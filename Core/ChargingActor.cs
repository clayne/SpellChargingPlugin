using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.Core
{
    public class ChargingActor
    {
        public Character Character { get; set; }
        public ChargingSpell[] Spells { get; set; }

        public ChargingActor(Character character)
        {
            Character = character;
            Spells = new ChargingSpell[2]; // 0 Left, 1 Right
        }

        public void Update(float diff)
        {
            var leftSpell = GetSpell(EquippedSpellSlots.LeftHand);
            var rightSpell = GetSpell(EquippedSpellSlots.RightHand);

            if(Spells[0].Spell != leftSpell)
            {
                Spells[0]?.Reset();
                Spells[0] = leftSpell != null ? new ChargingSpell(this, leftSpell) : null;
            }
            if (Spells[1].Spell != rightSpell)
            {
                Spells[1]?.Reset();
                Spells[1] = rightSpell != null ? new ChargingSpell(this, rightSpell) : null;
            }

            foreach (var spell in Spells)
                spell?.CurrentState.Update(diff);
        }

        private SpellItem GetSpell(EquippedSpellSlots hand)
        {
            return Character.GetMagicCaster(hand).CastItem as SpellItem;
        }
    }
}
