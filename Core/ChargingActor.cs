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

        public void Update(float elapsedSeconds)
        {
            var leftSpell = SpellHelper.GetHandSpellState(Character, EquippedSpellSlots.LeftHand)?.Spell;
            var rightSpell = SpellHelper.GetHandSpellState(Character, EquippedSpellSlots.RightHand)?.Spell;

            if (leftSpell == null)
            {
                DebugHelper.Print($"no left spell");
                if (Spells[0] != null)
                {
                    Spells[0]?.Reset();
                    Spells[0] = null;
                }
            }
            else if (leftSpell.FormId != Spells[0]?.Spell?.FormId)
            {
                Spells[0]?.Reset();
                DebugHelper.Print($"Assign left = {leftSpell.Name}");
                Spells[0] = new ChargingSpell(this, leftSpell, EquippedSpellSlots.LeftHand);
            }

            if (rightSpell == null)
            {
                DebugHelper.Print($"no right spell");
                if (Spells[1] != null)
                {
                    Spells[1]?.Reset();
                    Spells[1] = null;
                }
            }
            else if (rightSpell.FormId != Spells[1]?.Spell?.FormId)
            {
                Spells[1]?.Reset();
                DebugHelper.Print($"Assign right = {rightSpell.Name}");
                Spells[1] = new ChargingSpell(this, rightSpell, EquippedSpellSlots.RightHand);
            }

            foreach (var spell in Spells)
                spell?.CurrentState.Update(elapsedSeconds);
        }
    }
}
