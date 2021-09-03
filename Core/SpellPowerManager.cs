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

                RefreshPower(eff, basePower, modifier);
                RefreshArea(eff, basePower, modifier);

                if (!_isConcentration)
                    continue;
                // PeakValueMod spells (like Oakflesh) don't behave properly when updated in this manner
                if (eff.Effect.Archetype == Archetypes.PeakValueMod)
                    continue;

                RefreshActiveEffects(eff, basePower, modifier);
            }
        }


        /// <summary>
        /// Experimental buff to area (may not work)
        /// </summary>
        /// <param name="eff"></param>
        private void RefreshArea(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            // This easy? Needs testing. 
            eff.Area = (int)(basePower.Area * modifier);
            // What about projectile/visual effect scaling?
            //ScaleVisual(_managedSpell);
        }

        /// <summary>
        /// Refresh ActiveEffects on targets affected by this effect (should only be needed with Concentration type spells)
        /// </summary>
        /// <param name="eff"></param>
        private void RefreshActiveEffects(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            // Get all the actors affected by this effect
            var myFID = eff.Effect.FormId;
            var myActiveEffects = ActiveEffectTracker.Instance
                .Tracked()
                .ForBaseEffect(myFID)
                .FromOffender(PlayerCharacter.Instance.Cast<Character>())
                .Where(e => e.Invalid == false)
                .ToArray();
            if (myActiveEffects.Length == 0)
                return;
            DebugHelper.Print($"ActiveEffect : {eff.Effect.Name} affects {myActiveEffects.Length} targets");
            // Set Magnitude and call CalculateDurationAndMagnitude for each affected actor
            foreach (var victim in myActiveEffects)
            {
                if (MemoryObject.FromAddress<ActiveEffect>(victim.Effect) is ActiveEffect)
                {
                    DebugHelper.Print($"- Update on Victim {victim.Me.ToHexString()} MAG: {victim.Magnitude} -> {eff.Magnitude}");

                    victim.Magnitude = basePower.Magnitude * modifier;
                    Memory.InvokeCdecl(
                        Util.addr_CalculateDurationAndMagnitude,    //void __fastcall sub(
                        victim.Effect,                              //  ActiveEffect * a1, 
                        victim.Offender,                            //  Character * a2, 
                        victim.Me);                                 //  MagicTarget * a3);
                }
                else
                {
                    DebugHelper.Print($"- Effect {victim.Effect.ToHexString()} was invalid!");
                    victim.Invalid = true;
                }
            }
        }

        /// <summary>
        /// Adjust Magnitude and/or Duration according to Growth and Multiplier
        /// </summary>
        /// <param name="eff"></param>
        private void RefreshPower(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            // Boosting concentration duration AND magnitude would be a little too broken, even halved
            if (!_isConcentration)
                eff.Duration = (int)(basePower.Duration * (1.0f + modifier));
            eff.Magnitude = basePower.Magnitude * (1.0f + modifier);
        }
    }
}
