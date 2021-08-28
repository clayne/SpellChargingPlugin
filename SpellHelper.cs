using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpellChargingPlugin
{
    public static class SpellHelper
    {
        private static Dictionary<uint, SpellPower[]> _spellPowerCache = new Dictionary<uint, SpellPower[]>();

        public static SpellPower[] GetBaseSpellPower(SpellItem spell)
        {
            return _spellPowerCache[spell.FormId];
        }
        public static SpellPower[] DefineBaseSpellPower(SpellItem spell)
        {
            if (!_spellPowerCache.ContainsKey(spell.FormId))
                _spellPowerCache.Add(spell.FormId, spell.Effects.Select(eff => new SpellPower(eff.Magnitude, eff.Duration)).ToArray());
            return _spellPowerCache[spell.FormId];
        }

        public static EquippedSpellSlots? FindSpellInHand(Character character, SpellItem spell)
        {
            if (character == null || spell == null)
                return null;

            var leftHand = character.GetEquippedSpell(EquippedSpellSlots.LeftHand);
            if (leftHand != null && leftHand.Equals(spell))
                return EquippedSpellSlots.LeftHand;

            var rightHand = character.GetEquippedSpell(EquippedSpellSlots.RightHand);
            if (rightHand != null && rightHand.Equals(spell))
                return EquippedSpellSlots.RightHand;

            return null;
        }

        public static SpellItem GetSpellInHand(Character character, EquippedSpellSlots hand)
        {
            return character?.GetEquippedSpell(hand);
        }
        public static SpellHandState? GetHandSpellState(Character character, EquippedSpellSlots hand)
        {
            if (character == null)
                return null;
            var spellInHand = character.GetEquippedSpell(hand);
            if (spellInHand == null)
                return null;
            var spellState = character.GetMagicCaster(hand).State;
            return (spellState, spellInHand);
        }
    }

    public struct SpellHandState
    {
        public MagicCastingStates State;
        public SpellItem Spell;

        public SpellHandState(MagicCastingStates state, SpellItem spell)
        {
            State = state;
            Spell = spell;
        }

        public override bool Equals(object obj)
        {
            return obj is SpellHandState other &&
                   State == other.State &&
                   EqualityComparer<SpellItem>.Default.Equals(Spell, other.Spell);
        }

        public override int GetHashCode()
        {
            int hashCode = -374139281;
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<SpellItem>.Default.GetHashCode(Spell);
            return hashCode;
        }

        public void Deconstruct(out MagicCastingStates state, out SpellItem spell)
        {
            state = State;
            spell = Spell;
        }

        public static implicit operator (MagicCastingStates State, SpellItem Spell)(SpellHandState value)
        {
            return (value.State, value.Spell);
        }

        public static implicit operator SpellHandState((MagicCastingStates State, SpellItem Spell) value)
        {
            return new SpellHandState(value.State, value.Spell);
        }
    }
}
