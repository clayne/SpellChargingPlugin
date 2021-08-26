using NetScriptFramework.SkyrimSE;
using System.Collections.Generic;
using System.Linq;

namespace SpellChargingPlugin
{
    public static class SpellHelper
    {
        private static Dictionary<SpellItem, SpellPower[]> _spellBasePower = new Dictionary<SpellItem, SpellPower[]>();

        public static void CacheSpellBasePower(SpellItem spell)
        {
            if (_spellBasePower.ContainsKey(spell))
                return;
            _spellBasePower.Add(spell, spell.Effects?.Select(eff => new SpellPower(eff.Magnitude, eff.Duration)).ToArray());
        }
        public static SpellPower[] GetSpellBasePower(SpellItem spell)
        {
            _spellBasePower.TryGetValue(spell, out SpellPower[] ret);
            return ret;
        }

        public static EquippedSpellSlots GetEquippedHand(Character character, SpellItem spell)
        {
            if (character == null || spell == null)
                return EquippedSpellSlots.Other;

            var leftHand = character.GetMagicCaster(EquippedSpellSlots.LeftHand);
            if (leftHand != null && leftHand.CastItem.Equals(spell))
                return EquippedSpellSlots.LeftHand;

            var rightHand = character.GetMagicCaster(EquippedSpellSlots.RightHand);
            if (rightHand != null && rightHand.CastItem.Equals(spell))
                return EquippedSpellSlots.RightHand;

            return EquippedSpellSlots.Other;
        }
        public static MagicCastingStates GetCurrentCastingState(Character character, SpellItem spell)
        {
            if (character == null)
                return MagicCastingStates.None;
            if (spell == null)
                return MagicCastingStates.None;

            for (int i = 0; i <= 2; i++)
            {
                var caster = character.GetMagicCaster((EquippedSpellSlots)i);
                if (caster == null)
                    continue;

                var item = caster.CastItem;
                if (item == null || !item.Equals(spell))
                    continue;

                return caster.State;
            }

            return MagicCastingStates.None;
        }
    }

}
