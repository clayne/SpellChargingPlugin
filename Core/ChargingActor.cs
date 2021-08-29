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
        public Character Character => PlayerCharacter.Instance;
        public ChargingSpell[] Spells { get; set; } // 0 Left, 1 Right

        public ChargingActor()
        {
            Spells = new ChargingSpell[2];
        }
        
        // TODO: there's probably a more efficient way to check for equipped item changes besides manually checking
        public void Update(float elapsedSeconds)
        {
            var leftSpell = SpellHelper.GetSpellInHand(Character, EquippedSpellSlots.LeftHand);
            var rightSpell = SpellHelper.GetSpellInHand(Character, EquippedSpellSlots.RightHand);

            if (leftSpell != null)
            {
                if (leftSpell.FormId != Spells[0]?.Spell.FormId)
                    ResetAssign(leftSpell, ref Spells[0], EquippedSpellSlots.LeftHand, leftSpell.EquipSlot.FormId == 0x00013F45);
                Spells[0].Update(elapsedSeconds);
            }

            if (rightSpell != null)
            {
                if (rightSpell.FormId != Spells[1]?.Spell.FormId)
                    ResetAssign(rightSpell, ref Spells[1], EquippedSpellSlots.RightHand);
                Spells[1].Update(elapsedSeconds);
            }
        }

        private void ResetAssign(SpellItem spell, ref ChargingSpell chargingSpell, EquippedSpellSlots slot, bool isTwoHand = false)
        {
            DebugHelper.Print($"Hand: {slot} Spell: {chargingSpell?.Spell?.Name} -> {spell.Name}");
            if (chargingSpell != null)
                chargingSpell.Reset();
            chargingSpell = new ChargingSpell(this, spell, slot, isTwoHand);
        }
    }
}
