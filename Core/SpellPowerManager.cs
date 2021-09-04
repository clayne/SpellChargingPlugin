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
        public float Growth { get => _growth; set { _growth = value; Refresh(); } }

        /// <summary>
        /// Growth multiplier ("current charges")
        /// </summary>
        public float Multiplier { get => _multiplier; set { _multiplier = value; Refresh(); } }

        private float _growth;
        private float _multiplier;
        private ChargingSpell _managedSpell;
        private bool _isConcentration;

        private SpellPowerManager() { }
        public static SpellPowerManager Create(ChargingSpell spell)
        {
            if (spell == null)
                throw new ArgumentException("[SpellPowerManager] Can't assign NULL spell!");

            var ret = new SpellPowerManager()
            {
                _managedSpell = spell,
                _growth = 0.0f,
                _multiplier = 0.0f,
                _isConcentration = spell.Spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration,
            };
            return ret;
        }

        /// <summary>
        /// Refresh Spell magnitudes and other associated attributes and effects
        /// </summary>
        private void Refresh()
        {
            foreach (var eff in _managedSpell.Spell.Effects)
            {
                var basePower = SpellHelper.GetBasePower(eff);
                float baseModifier = 1f + _growth * _multiplier; // for power, linear gains
                float adjustedModifier = 1f + (float)Math.Log10(baseModifier) * 0.5f; // for area and speed, diminishing gains, less steep

                //DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}] Eff: {eff.Effect.Name} Mod: {modifier}");
                BoostPower(eff, basePower, baseModifier);
                BoostArea(eff, basePower, adjustedModifier);
                BoostSpeed(eff, basePower, adjustedModifier);

                if (_isConcentration)
                    RefreshActiveEffects(eff, basePower, baseModifier);
            }
        }

        /// <summary>
        /// Experimental buff to area (may not work)
        /// </summary>
        /// <param name="eff"></param>
        private void BoostArea(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            // all pointer values extracted from SKSE 2.0.19 GameObjects.h
            //IntPtr fRangePtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x0C; // range probably doesn't need to be increased

            if (eff.Area > 0)
            {
                eff.Area = (int)(basePower.Area * modifier);

                // Explosion area too, if it has one
                if (basePower.ExplosionRadius != null)
                {
                    IntPtr fExplosionRadPtr = eff.Effect.Explosion.ExplosionData.Address + 0x38;
                    Memory.WriteFloat(fExplosionRadPtr, basePower.ExplosionRadius.Value * modifier);
                }

                // projecile collision needs to be made larger too for things like Ice Storm
                if (basePower.CollisionRadius != null)
                {

                    IntPtr fCollisionRadiusPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x6C;
                    Memory.WriteFloat(fCollisionRadiusPtr, basePower.CollisionRadius.Value * modifier);
                }

                if (basePower.ConeSpread != null)
                {
                    IntPtr fConeSpreadPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x68;
                    Memory.WriteFloat(fConeSpreadPtr, basePower.ConeSpread.Value * modifier);
                }
            }
        }

        /// <summary>
        /// Probably unbalanced, but makes wasting mana on charges less punishing by allowing you to hit targets easier
        /// </summary>
        /// <param name="eff"></param>
        /// <param name="basePower"></param>
        /// <param name="modifier"></param>
        private void BoostSpeed(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            if (basePower.Speed != null)
            {
                IntPtr fSpeedPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x08;
                Memory.WriteFloat(fSpeedPtr, basePower.Speed.Value * modifier);
            }
        }

        /// <summary>
        /// toys
        /// </summary>
        /// <param name="eff"></param>
        private void BoostMisc(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            if (eff.Effect.MagicProjectile?.ProjectileData == null)
                return;
            DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}:{eff.Effect.Name}] RefreshMisc");

            IntPtr fRangePtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x0C;
            IntPtr fCollisionRadiusPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x6C;
        }

        /// <summary>
        /// Refresh ActiveEffects on targets affected by this effect (should only be needed with Concentration type spells)
        /// </summary>
        /// <param name="eff"></param>
        private void RefreshActiveEffects(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            // PeakValueMod spells (like Oakflesh) don't behave properly when updated in this manner
            if (eff.Effect.Archetype == Archetypes.PeakValueMod)
                return;

            DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}:{eff.Effect.Name}] RefreshActiveEffects");
            // Get all the actors affected by this effect
            var myFID = eff.Effect.FormId;
            var myActiveEffects = ActiveEffectTracker.Instance
                .Tracked()
                .ForBaseEffect(myFID)
                .FromOffender(_managedSpell.Holder.Actor.Cast<Character>())
                .Where(e => e.Invalid == false)
                .ToArray();
            if (myActiveEffects.Length == 0)
                return;
            DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}] Eff : {eff.Effect.Name} affects {myActiveEffects.Length} targets");
            // Set Magnitude and call CalculateDurationAndMagnitude for each affected actor
            foreach (var victim in myActiveEffects)
            {
                if (MemoryObject.FromAddress<ActiveEffect>(victim.Effect) is ActiveEffect)
                {
                    DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}:{eff.Effect.Name}] Update on Victim {victim.Me.ToHexString()} MAG: {victim.Magnitude} -> {eff.Magnitude}");

                    victim.Magnitude = basePower.Magnitude * modifier;
                    Memory.InvokeCdecl(
                        Util.addr_CalculateDurationAndMagnitude,    //void __fastcall sub(
                        victim.Effect,                              //  ActiveEffect * a1, 
                        victim.Offender,                            //  Character * a2, 
                        victim.Me);                                 //  MagicTarget * a3);
                }
                else
                {
                    DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}:{eff.Effect.Name}] Invalid ActiveEffect pointer {victim.Effect.ToHexString()}!");
                    victim.Invalid = true;
                }
            }
        }

        /// <summary>
        /// Adjust Magnitude OR Duration according to Growth and Multiplier
        /// </summary>
        /// <param name="eff"></param>
        private void BoostPower(EffectItem eff, SpellHelper.EffectPower basePower, float modifier)
        {
            //DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}:{eff.Effect.Name}] RefreshPower");

            switch (_managedSpell.Holder.Mode)
            {
                case ChargingActor.OperationMode.Magnitude:
                    eff.Magnitude = basePower.Magnitude * modifier;
                    break;
                case ChargingActor.OperationMode.Duration:
                    eff.Duration = (int)(basePower.Duration * modifier);
                    break;
                case ChargingActor.OperationMode.Both:
                    if(!_isConcentration) // would be too OP
                        eff.Duration = (int)(basePower.Duration * modifier);
                    eff.Magnitude = basePower.Magnitude * modifier;
                    break;
            }
            //if (eff.Magnitude > 0f)
            //{
            //    eff.Magnitude = basePower.Magnitude * modifier;
            //}
            //else if (eff.Duration > 0)
            //{
            //    eff.Duration = (int)(basePower.Duration * modifier);
            //}
        }
    }
}
