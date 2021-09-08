using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using static NetScriptFramework.SkyrimSE.ActiveEffect;
using static SpellChargingPlugin.SpellHelper;

namespace SpellChargingPlugin.Core
{
    /// <summary>
    /// Handles Effect Magnitude and Duration adjustments for a single Spell
    /// All pointer values extracted from SKSE 2.0.19 GameObjects.h
    /// </summary>
    public class SpellPowerManager
    {
        /// <summary>
        /// Percentage gain of power ("power per charge")
        /// </summary>
        public float Growth { get; set; }

        private ChargingSpell _managedSpell;
        private bool _isConcentration;
        private bool _hasDuration, _hasMagnitude;
        private bool _needReset = false;

        // just leave these here?
        float? _baseRange;
        float _modifiedRange;

        private SpellPowerManager() { }
        public static SpellPowerManager Create(ChargingSpell spell)
        {
            if (spell == null)
                throw new ArgumentException("[SpellPowerManager] Can't assign NULL spell!");

            var ret = new SpellPowerManager()
            {
                _managedSpell = spell,
                _isConcentration = spell.Spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration,
                _hasDuration = SpellHelper.HasDuration(spell.Spell),
                _hasMagnitude = SpellHelper.HasMagnitude(spell.Spell),

                _baseRange = spell.Spell.Effects.FirstOrDefault()?.Effect?.MagicProjectile?.ProjectileData?.Range,
            };
            return ret;
        }

        /// <summary>
        /// Grow Spell magnitudes and other associated attributes and effects by one rank
        /// </summary>
        public void IncreasePower()
        {
            _needReset = true;
            float adjustedGrowth = Growth * (0.5f / (float)Math.Log10(_managedSpell.ChargeLevel * _managedSpell.ChargeLevel + 1));
            foreach (var eff in _managedSpell.Spell.Effects)
            {
                var basePower = GetBasePower(eff);
                EffectPower mod = GetModifiedPower(eff);

                switch (_managedSpell.Holder.Mode)
                {
                    case ChargingActor.OperationMode.Magnitude:
                        if (_hasMagnitude)
                            mod.Magnitude += basePower.Magnitude * Growth;
                        else if(_hasDuration)
                            mod.Duration += basePower.Duration * Growth;
                        break;
                    case ChargingActor.OperationMode.Duration:
                        if (_hasDuration)
                            mod.Duration += basePower.Duration * Growth;
                        else if (_hasMagnitude)
                            mod.Magnitude += basePower.Magnitude * Growth;
                        break;
                }

                mod.Area += basePower.Area * adjustedGrowth;
                if (mod.CollisionRadius != null)
                    mod.CollisionRadius += basePower.CollisionRadius * adjustedGrowth;
                if (mod.ConeSpread != null)
                    mod.ConeSpread += basePower.ConeSpread * adjustedGrowth;
                if (mod.ExplosionRadius != null)
                    mod.ExplosionRadius += basePower.ExplosionRadius * adjustedGrowth;
                if (mod.Speed != null)
                    mod.Speed += basePower.Speed * adjustedGrowth;

                //DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}] Eff: {eff.Effect.Name} Mod: {modifier}");
                ApplyModPower(eff, mod);
                ApplyModArea(eff, mod);
                ApplyModSpeed(eff, mod);

                if (_isConcentration)
                    ApplyModActiveEffects(eff, mod);
            }

            if (_baseRange > 1f)
                _modifiedRange += _baseRange.Value * adjustedGrowth;
            ApplyModRange(_modifiedRange);
        }

        /// <summary>
        /// Projectile (and maybe conal concentration?) range, scaled proportionally to speed
        /// </summary>
        /// <param name="bonusRange"></param>
        private void ApplyModRange(float bonusRange)
        {
            if (_modifiedRange > 0f)
            {
                IntPtr fRangePtr = _managedSpell.Spell.Effects[0].Effect.MagicProjectile.ProjectileData.Address + 0x0C;
                Memory.WriteFloat(fRangePtr, _modifiedRange);
            }
        }

        /// <summary>
        /// Reset all spell effect magnitudes etc to their base power level
        /// </summary>
        public void ResetPower()
        {
            if (!_needReset)
                return;
            foreach (var eff in _managedSpell.Spell.Effects)
            {
                var mod = GetModifiedPower(eff);
                mod.ResetTo(GetBasePower(eff));
                ApplyModPower(eff, mod);
                ApplyModArea(eff, mod);
                ApplyModSpeed(eff, mod);
            }
            _needReset = false;
        }

        /// <summary>
        /// Adjust Magnitude / Duration
        /// </summary>
        /// <param name="eff"></param>
        private void ApplyModPower(EffectItem eff, EffectPower modifiedPower)
        {
            //DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}:{eff.Effect.Name}] RefreshPower");
            eff.Magnitude = modifiedPower.Magnitude;
            eff.Duration = (int)modifiedPower.Duration;
        }

        /// <summary>
        /// Experimental buff to area (may not work)
        /// </summary>
        /// <param name="eff"></param>
        private void ApplyModArea(EffectItem eff, EffectPower modifiedPower)
        {
            if (modifiedPower.Area > 0)
            {
                eff.Area = (int)(modifiedPower.Area);
                // Explosion area too, if it has one
                if (modifiedPower.ExplosionRadius != null)
                {
                    IntPtr fExplosionRadPtr = eff.Effect.Explosion.ExplosionData.Address + 0x38;
                    Memory.WriteFloat(fExplosionRadPtr, modifiedPower.ExplosionRadius.Value);
                }
                // projecile collision needs to be made larger too for things like Ice Storm
                if (modifiedPower.CollisionRadius != null)
                {
                    IntPtr fCollisionRadiusPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x6C;
                    Memory.WriteFloat(fCollisionRadiusPtr, modifiedPower.CollisionRadius.Value);
                }
                if (modifiedPower.ConeSpread != null)
                {
                    IntPtr fConeSpreadPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x68;
                    Memory.WriteFloat(fConeSpreadPtr, modifiedPower.ConeSpread.Value);
                }
            }
        }

        /// <summary>
        /// Probably unbalanced, but makes wasting mana on charges less punishing by allowing you to hit targets easier
        /// </summary>
        /// <param name="eff"></param>
        /// <param name="basePower"></param>
        /// <param name="modifier"></param>
        private void ApplyModSpeed(EffectItem eff, EffectPower modifiedPower)
        {
            if (modifiedPower.Speed != null)
            {
                IntPtr fSpeedPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x08;
                Memory.WriteFloat(fSpeedPtr, modifiedPower.Speed.Value);
            }
        }

        /// <summary>
        /// toys
        /// </summary>
        /// <param name="eff"></param>
        private void ApplyModMisc(EffectItem eff, EffectPower modifiedPower)
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
        private void ApplyModActiveEffects(EffectItem eff, EffectPower modifiedPower)
        {
            //DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}:{eff.Effect.Name}] RefreshActiveEffects");
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
                try
                {
                    if (MemoryObject.FromAddress<ActiveEffect>(victim.Effect) is ActiveEffect activeEffect)
                    {
                        DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}:{eff.Effect.Name}] Update on Victim {victim.Me.ToHexString()} MAG: {victim.Magnitude} -> {eff.Magnitude}");

                        victim.Magnitude = modifiedPower.Magnitude;
                        victim.Duration = modifiedPower.Duration;

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
                catch (Exception ex)
                {
                    DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Spell.Name}:{eff.Effect.Name}] THREW EXCEPTION! {ex.Message}");
                    victim.Invalid = true;
                }
            }
        }
    }
}
