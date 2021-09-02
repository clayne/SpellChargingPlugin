using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using static NetScriptFramework.SkyrimSE.ActiveEffect;

namespace SpellChargingPlugin.Core
{
    /// <summary>
    /// Handles Effect Magnitude and Duration adjustments for a single Spell
    /// </summary>
    public class SpellPowerManager
    {
        /// <summary>
        /// Percentage gain of power ("power per charge")
        /// </summary>
        public float Growth { get => _growth; set { _growth = value; Update(0.0f); } }

        /// <summary>
        /// Growth multiplier ("current charges")
        /// </summary>
        public float Multiplier { get => _multiplier; set { _multiplier = value; Update(0.0f); } }

        private float _growth;
        private float _multiplier;
        private SpellItem _managedSpell;
        private bool _isConcentration;

        private SpellPowerManager() { }
        public static SpellPowerManager CreateFor(SpellItem spell)
        {
            if (spell == null)
                throw new ArgumentException("Can't assign NULL spell!");

            var ret = new SpellPowerManager()
            {
                _managedSpell = spell,
                _growth = 0.0f,
                _multiplier = 0.0f,
                _isConcentration = spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration,
            };
            return ret;
        }

        /// <summary>
        /// Refresh Spell magnitudes. Normally only necessary after updating Growth or Power, but those Properties already call this.
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        private void Update(float elapsedSeconds)
        {
            foreach (var eff in _managedSpell.Effects)
            {
                var basePower = SpellHelper.GetBasePower(eff);
                var modifier = _growth * _multiplier;

                if (modifier > 0.0f && Settings.Instance.HalfPowerWhenMagAndDur && !_isConcentration)
                {
                    bool hasMag = eff.Magnitude > 0f;
                    bool hasDur = eff.Duration > 0;
                    modifier *= hasMag && hasDur ? 0.5f : 1f;
                }
                if (!_isConcentration) eff.Duration = (int)(basePower.Duration * (1.0f + modifier));
                eff.Magnitude = basePower.Magnitude * (1.0f + modifier);

                if (!SpellCharging._trackedEffects.Any())
                    return;


                IntPtr aEff = IntPtr.Zero;
                HashSet<IntPtr> invalids = new HashSet<IntPtr>();
                lock (SpellCharging._trackedEffects)
                {
                    foreach (var kv in SpellCharging._trackedEffects)
                    {
                        try
                        {
                            ActiveEffect ef = MemoryObject.FromAddress<ActiveEffect>(kv.Key);
                            if (!(ef is ActiveEffect))
                                invalids.Add(kv.Key);
                            else if(ef.EffectData.Effect.FormId == eff.Effect.FormId)
                            {
                                aEff = kv.Key;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugHelper.Print(ex.Message);
                            invalids.Add(kv.Key);
                        }
                    }
                    foreach (var item in invalids)
                    {
                        SpellCharging._trackedEffects.Remove(item);
                    }
                }
                if (aEff == IntPtr.Zero)
                    return;

                IntPtr addr_CalculateDurationAndMagnitude = new IntPtr(0x14053DF40).FromBase();

                if (SpellCharging._trackedEffects.TryGetValue(aEff, out var victims))
                {
                    victims.Magnitude = eff.Magnitude;
                    for (int i = 0; i < victims.Victims.Count; i++)
                    {
                        var tracked = victims.Victims[i];
                        DebugHelper.Print($"ActiveEffect : {eff.Effect.Name}");
                        DebugHelper.Print("- - Invoke addr_CalculateDurationAndMagnitude for update!");
                        Memory.InvokeCdecl(
                            addr_CalculateDurationAndMagnitude,
                            aEff,
                            PlayerCharacter.Instance.Cast<Character>(),
                            tracked);
                    }
                }
            }

        }
    }
}
