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
        public Character Actor { get; }
        public ChargingSpell LeftSpell { get; private set; }
        public ChargingSpell RightSpell { get; private set; }

        bool _leftEqualsRight = false;

        public ChargingActor(Character character)
        {
            Actor = character;
        }

        public void Update(float elapsedSeconds)
        {
            AssignSpellsIfNecessary();

            LeftSpell?.Update(elapsedSeconds);
            if(!_leftEqualsRight)
                RightSpell?.Update(elapsedSeconds);
        }

        /// <summary>
        /// Check if the character has spells equipped and assign them to their appropriate <cref>ChargingSpell</cref> slots.
        /// Also take care of cleaning up the previous <cref>ChargingSpell</cref> if overwriting.
        /// </summary>
        private void AssignSpellsIfNecessary()
        {
            var leftSpell = SpellHelper.GetSpell(Actor, EquippedSpellSlots.LeftHand);

            if (leftSpell == null && LeftSpell != null)
                LeftSpell = ClearSpell(LeftSpell);
            else if (LeftSpell?.Equals(leftSpell) != true)
            {
                LeftSpell = SetSpell(leftSpell, EquippedSpellSlots.LeftHand);
                _leftEqualsRight = LeftSpell.IsTwoHanded;
            }

            // Can skip right hand check and assignment when using master-tier or other two-handed spells.
            if (!_leftEqualsRight)
            {
                var rightSpell = SpellHelper.GetSpell(Actor, EquippedSpellSlots.RightHand);

                if (rightSpell == null && RightSpell != null)
                    RightSpell = ClearSpell(RightSpell);
                else if (RightSpell?.Equals(rightSpell) != true)
                    RightSpell = SetSpell(rightSpell, EquippedSpellSlots.RightHand);
            }
        }

        private ChargingSpell ClearSpell(ChargingSpell spell)
        {
            spell.Reset();
            return null;
        }

        private ChargingSpell SetSpell(SpellItem spellItem, EquippedSpellSlots handSlot)
        {
            DebugHelper.Print($"Hand: {handSlot} -> {spellItem.Name}");
            var isTwoHandedSpell = spellItem.EquipSlot?.FormId == Settings.Instance.EquipBothFormID;
            var ret = new ChargingSpell(this, spellItem, isTwoHandedSpell ? EquippedSpellSlots.Other : handSlot);
            return ret;
        }
    }
}
