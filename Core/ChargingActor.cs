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
        public ChargingSpell[] Spells { get; set; } // 0 Left, 1 Right

        public ChargingActor(Character character)
        {
            Character = character;
            Spells = new ChargingSpell[2]; 
        }

        public void Update(float diff)
        {
            var leftSpell = SpellHelper.GetHandSpellState(Character, EquippedSpellSlots.LeftHand)?.Spell;
            var rightSpell = SpellHelper.GetHandSpellState(Character, EquippedSpellSlots.RightHand)?.Spell;

            if(Spells[0].Spell != leftSpell)
            {
                Spells[0]?.Reset();
                Spells[0] = leftSpell != null ? new ChargingSpell(this, leftSpell, EquippedSpellSlots.LeftHand) : null;
            }
            if (Spells[1].Spell != rightSpell)
            {
                Spells[1]?.Reset();
                Spells[1] = rightSpell != null ? new ChargingSpell(this, rightSpell, EquippedSpellSlots.RightHand) : null;
            }

            foreach (var spell in Spells)
                spell?.CurrentState.Update(diff);
        }
    }
}
