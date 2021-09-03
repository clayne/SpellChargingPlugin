using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static NetScriptFramework.SkyrimSE.ActiveEffect;

namespace SpellChargingPlugin
{
    public static class SpellHelper
    {
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
        /// Holds Spell Magnitude and Duration
        /// </summary>
        public readonly struct EffectPower
        {
            public readonly float Magnitude;
            public readonly int Duration;
            public readonly float Area;

            public EffectPower(float magnitude, int duration, float area)
            {
                Magnitude = magnitude;
                Duration = duration;
                Area = area;
            }
        }

        private static Dictionary<EffectItem, EffectPower> _baseEffectPowers = new Dictionary<EffectItem, EffectPower>();

        /// <summary>
        /// Get the BASE Magniture and/or Durations for the given Effect
        /// </summary>
        /// <param name="effectItem"></param>
        /// <returns>EffectPower</returns>
        public static EffectPower GetBasePower(EffectItem effectItem)
        {
            if (!_baseEffectPowers.ContainsKey(effectItem))
                _baseEffectPowers.Add(
                    effectItem, 
                    new EffectPower(
                        effectItem.Magnitude, 
                        effectItem.Duration,
                        effectItem.Area));
            return _baseEffectPowers[effectItem];
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
    }
}
