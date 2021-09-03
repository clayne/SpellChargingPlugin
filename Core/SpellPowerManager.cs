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
                throw new ArgumentException("[SpellPowerManager] Can't assign NULL spell!");

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

                DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}] Eff: {eff.Effect.Name}");
                RefreshPower(eff, basePower, modifier);
                RefreshArea(eff, basePower, modifier);
                RefreshMisc(eff);

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
        }

        /// <summary>
        /// toys
        /// </summary>
        /// <param name="eff"></param>
        private void RefreshMisc(EffectItem eff)
        {
            // What about projectile/visual effect scaling?
            IntPtr fSpeedPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x08;
            IntPtr fRangePtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x0C;
            IntPtr fCollisionRadiusPtr = eff.Effect.MagicProjectile.ProjectileData.Address + 0x6C;

            // See what happens here
            try
            {
                DebugHelper.Print($"[SpellPowerManager] fSpeedPtr (before): {NativeCrashLog.GetValueInfo(fSpeedPtr)}");
                DebugHelper.Print($"[SpellPowerManager] fRangePtr (before): {NativeCrashLog.GetValueInfo(fRangePtr)}");
                DebugHelper.Print($"[SpellPowerManager] fCollisionRadiusPtr (before): {NativeCrashLog.GetValueInfo(fCollisionRadiusPtr)}");

                Memory.WriteFloat(fSpeedPtr, 7777);
                Memory.WriteFloat(fRangePtr, 7);
                Memory.WriteFloat(fCollisionRadiusPtr, 7777);

                DebugHelper.Print($"[SpellPowerManager] fSpeedPtr (after): {NativeCrashLog.GetValueInfo(fSpeedPtr)}");
                DebugHelper.Print($"[SpellPowerManager] fRangePtr (after): {NativeCrashLog.GetValueInfo(fRangePtr)}");
                DebugHelper.Print($"[SpellPowerManager] fCollisionRadiusPtr (after): {NativeCrashLog.GetValueInfo(fCollisionRadiusPtr)}");
            }
            catch (Exception ex)
            {
                DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}] Failed to write to projectile data! {ex.Message}");
            }

            try
            {
                // What are these values??
                IntPtr unk00 = eff.Effect.Explosion.ExplosionData.Address + 0x00;
                IntPtr unk08 = eff.Effect.Explosion.ExplosionData.Address + 0x08;
                IntPtr unk10 = eff.Effect.Explosion.ExplosionData.Address + 0x10;
                IntPtr unk18 = eff.Effect.Explosion.ExplosionData.Address + 0x18;
                IntPtr unk20 = eff.Effect.Explosion.ExplosionData.Address + 0x20;
                IntPtr unk28 = eff.Effect.Explosion.ExplosionData.Address + 0x28;
                IntPtr unk30 = eff.Effect.Explosion.ExplosionData.Address + 0x30;
                IntPtr unk34 = eff.Effect.Explosion.ExplosionData.Address + 0x34;
                IntPtr unk38 = eff.Effect.Explosion.ExplosionData.Address + 0x38;
                IntPtr unk3C = eff.Effect.Explosion.ExplosionData.Address + 0x3C;
                IntPtr unk40 = eff.Effect.Explosion.ExplosionData.Address + 0x40;
                IntPtr unk44 = eff.Effect.Explosion.ExplosionData.Address + 0x44;
                IntPtr unk48 = eff.Effect.Explosion.ExplosionData.Address + 0x48;
                IntPtr unk4C = eff.Effect.Explosion.ExplosionData.Address + 0x4C;

                DebugHelper.Print($"__unk00 as float : {Memory.ReadFloat(unk00)} ({NativeCrashLog.GetValueInfo(unk00)})");
                DebugHelper.Print($"__unk08 as float : {Memory.ReadFloat(unk08)} ({NativeCrashLog.GetValueInfo(unk08)})");
                DebugHelper.Print($"__unk10 as float : {Memory.ReadFloat(unk10)} ({NativeCrashLog.GetValueInfo(unk10)})");
                DebugHelper.Print($"__unk18 as float : {Memory.ReadFloat(unk18)} ({NativeCrashLog.GetValueInfo(unk18)})");
                DebugHelper.Print($"__unk20 as float : {Memory.ReadFloat(unk20)} ({NativeCrashLog.GetValueInfo(unk20)})");
                DebugHelper.Print($"__unk28 as float : {Memory.ReadFloat(unk28)} ({NativeCrashLog.GetValueInfo(unk28)})");
                DebugHelper.Print($"__unk30 as float : {Memory.ReadFloat(unk30)} ({NativeCrashLog.GetValueInfo(unk30)})");
                DebugHelper.Print($"__unk34 as float : {Memory.ReadFloat(unk34)} ({NativeCrashLog.GetValueInfo(unk34)})");
                DebugHelper.Print($"__unk38 as float : {Memory.ReadFloat(unk38)} ({NativeCrashLog.GetValueInfo(unk38)})");
                DebugHelper.Print($"__unk3C as float : {Memory.ReadFloat(unk3C)} ({NativeCrashLog.GetValueInfo(unk3C)})");
                DebugHelper.Print($"__unk40 as float : {Memory.ReadFloat(unk40)} ({NativeCrashLog.GetValueInfo(unk40)})");
                DebugHelper.Print($"__unk44 as float : {Memory.ReadFloat(unk44)} ({NativeCrashLog.GetValueInfo(unk44)})");
                DebugHelper.Print($"__unk48 as float : {Memory.ReadFloat(unk48)} ({NativeCrashLog.GetValueInfo(unk48)})");
                DebugHelper.Print($"__unk4C as float : {Memory.ReadFloat(unk4C)} ({NativeCrashLog.GetValueInfo(unk4C)})");
            }
            catch (Exception ex)
            {
                DebugHelper.Print($"Failed to read to explosion data! {ex.Message}");
            }
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
            DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}] Eff : {eff.Effect.Name} affects {myActiveEffects.Length} targets");
            // Set Magnitude and call CalculateDurationAndMagnitude for each affected actor
            foreach (var victim in myActiveEffects)
            {
                if (MemoryObject.FromAddress<ActiveEffect>(victim.Effect) is ActiveEffect)
                {
                    DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}:{eff.Effect.Name}] Update on Victim {victim.Me.ToHexString()} MAG: {victim.Magnitude} -> {eff.Magnitude}");

                    victim.Magnitude = basePower.Magnitude * modifier;
                    Memory.InvokeCdecl(
                        Util.addr_CalculateDurationAndMagnitude,    //void __fastcall sub(
                        victim.Effect,                              //  ActiveEffect * a1, 
                        victim.Offender,                            //  Character * a2, 
                        victim.Me);                                 //  MagicTarget * a3);
                }
                else
                {
                    DebugHelper.Print($"[SpellPowerManager:{_managedSpell.Name}:{eff.Effect.Name}] Invalid ActiveEffect pointer {victim.Effect.ToHexString()}!");
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
