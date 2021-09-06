using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static NetScriptFramework.SkyrimSE.ActiveEffect;

namespace SpellChargingPlugin
{
    public static class SpellHelper
    {
        private static Dictionary<EffectItem, EffectPower> _baseEffectPowers = new Dictionary<EffectItem, EffectPower>();
        private static Dictionary<EffectItem, EffectPower> _modifiedPowers = new Dictionary<EffectItem, EffectPower>();

        /// <summary>
        /// Because the main attributes that define a spell's "power" are Magnitude and Duration, this will check whether a spell can even be considered a valid candidate for charging.
        /// If a spell has neither a duration nor a magnitude, it is not a valid charging spell (scripted/special).
        /// If it has no magnitude but a duration, and is a concentration spell, it is not a valid charging spell (most likely scripted).
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        internal static bool CanSpellBeCharged(SpellItem spell)
        {
            bool hasMagnitudeSomewhere = false;
            bool hasDurationSomewhere = false;
            foreach (var eff in spell.Effects)
            {
                hasMagnitudeSomewhere = hasMagnitudeSomewhere || eff.Magnitude > 0f;
                hasDurationSomewhere = hasDurationSomewhere || eff.Duration > 0;
            }
            if (spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration)
                hasDurationSomewhere = false;
            var ret = hasMagnitudeSomewhere || hasDurationSomewhere;
            DebugHelper.Print($"Spell {spell.Name} {(!ret ? "can't" : "can")} be charged.");
            return ret;
        }

        /// <summary>
        /// See if the spell's FIRST effect has a magnitude. Usually the first effect is the spell's defining effect, so this should be enough.
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        internal static bool HasMagnitude(SpellItem spell)
        {
            return spell.Effects.FirstOrDefault()?.Magnitude > 0f;
        }

        /// <summary>
        /// See if the spell's FIRST effect has a duration. Usually the first effect is the spell's defining effect, so this should be enough.
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        internal static bool HasDuration(SpellItem spell)
        {
            return spell.Effects.FirstOrDefault()?.Duration > 0f;
        }

        /// <summary>
        /// Get the BASE Magniture and/or Durations for the given Effect
        /// </summary>
        /// <param name="effectItem"></param>
        /// <returns>EffectPower</returns>
        public static EffectPower GetBasePower(EffectItem effectItem)
        {
            if(!_baseEffectPowers.TryGetValue(effectItem, out var ret))
                _baseEffectPowers.Add(effectItem, ret = new EffectPower()
                {
                    Magnitude       = effectItem.Magnitude,
                    Duration        = effectItem.Duration,
                    Area            = effectItem.Area,
                    Speed           = effectItem.Effect.MagicProjectile?.ProjectileData?.Speed,
                    ExplosionRadius = effectItem.Effect.Explosion?.ExplosionData?.Radius,
                    CollisionRadius = effectItem.Effect.MagicProjectile?.ProjectileData?.CollisionRadius,
                    ConeSpread      = effectItem.Effect.MagicProjectile?.ProjectileData?.ConeSpread,
                });
            return ret;
        }

        /// <summary>
        /// Returns the MODIFIED (base + bonus) effect power
        /// </summary>
        /// <param name="eff"></param>
        /// <returns></returns>
        public static EffectPower GetModifiedPower(EffectItem eff)
        {
            if (!_modifiedPowers.TryGetValue(eff, out var mod))
                _modifiedPowers.Add(eff, mod = new EffectPower()
                {
                    Magnitude       = eff.Magnitude,
                    Duration        = eff.Duration,
                    Area            = eff.Area,
                    Speed           = eff.Effect.MagicProjectile?.ProjectileData?.Speed,
                    ExplosionRadius = eff.Effect.Explosion?.ExplosionData?.Radius,
                    CollisionRadius = eff.Effect.MagicProjectile?.ProjectileData?.CollisionRadius,
                    ConeSpread      = eff.Effect.MagicProjectile?.ProjectileData?.ConeSpread,
                });
            return mod;
        }

        /// <summary>
        /// Self-explanatory. A more lightweight version of <cref>GetSpellAndState</cref> for when you don't need the State
        /// </summary>
        /// <param name="character"></param>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static SpellItem GetSpell(Character character, EquippedSpellSlots hand)
            => character?.GetEquippedSpell(hand);

        /// <summary>
        /// Self-explanatory.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static SpellHandState? GetSpellAndState(Character character, EquippedSpellSlots hand)
        {
            if (character == null)
                return null;
            var spellInHand = GetSpell(character, hand);
            if (spellInHand == null)
                return null;
            var spellState = character.GetMagicCaster(hand).State;
            return new SpellHandState (spellState, spellInHand, hand);
        }

        /// <summary>
        /// Represents a Spell and its State in an actor's hand
        /// </summary>
        public readonly struct SpellHandState
        {
            public readonly MagicCastingStates State;
            public readonly SpellItem Spell;
            public readonly EquippedSpellSlots Slot;

            public SpellHandState(MagicCastingStates state, SpellItem spell, EquippedSpellSlots slot)
            {
                State = state;
                Spell = spell;
                Slot = slot;
            }
        }

        

        /// <summary>
        /// Holds temporary spell power
        /// </summary>
        public sealed class EffectPower
        {
            public float Magnitude;
            public float Duration;
            public float Area;
            public float? Speed;
            public float? ExplosionRadius;
            public float? CollisionRadius;
            public float? ConeSpread;

            public void ResetTo(EffectPower basePower)
            {
                DebugHelper.Print($"[EffectPower:{GetHashCode()}] Reset to base {basePower.GetHashCode()}");
                Magnitude       = basePower.Magnitude;
                Duration        = basePower.Duration;
                Area            = basePower.Area;
                Speed           = basePower.Speed;
                ExplosionRadius = basePower.ExplosionRadius;
                CollisionRadius = basePower.CollisionRadius;
                ConeSpread      = basePower.ConeSpread;
            }
        }
    }
}
