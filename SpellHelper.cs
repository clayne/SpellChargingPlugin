using NetScriptFramework.SkyrimSE;
using System.Collections.Generic;
using System.Linq;

namespace SpellChargingPlugin
{
    public static class SpellHelper
    {
        public static EquippedSpellSlots? FindSpellInHand(Character character, SpellItem spell)
        {
            if (character == null || spell == null)
                return null;

            var leftHand = character.GetMagicCaster(EquippedSpellSlots.LeftHand);
            if (leftHand != null && leftHand.CastItem.Equals(spell))
                return EquippedSpellSlots.LeftHand;

            var rightHand = character.GetMagicCaster(EquippedSpellSlots.RightHand);
            if (rightHand != null && rightHand.CastItem.Equals(spell))
                return EquippedSpellSlots.RightHand;

            return null;
        }

        public static SpellHandState? GetHandSpellState(Character character, EquippedSpellSlots hand)
        {
            if (character == null)
                return null;
            var itemInHand = character.GetMagicCaster(hand);
            if (itemInHand == null)
                return null;
            var spellInHand = itemInHand.CastItem as SpellItem;
            if (spellInHand == null)
                return null;
            return (itemInHand.State, spellInHand);
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
