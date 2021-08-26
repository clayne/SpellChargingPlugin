using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    class SpellActorState
    {
        public Character Actor { get; set; }
        public SpellData LeftSpell { get; set; }
        public SpellData RightSpell { get; set; }

        public void OnUpdate(float diff)
        {
            if (LeftSpell != null)
            {
                UpdateState(LeftSpell);
                LeftSpell.OnUpdate(diff);
            }
            if (RightSpell != null)
            {
                UpdateState(RightSpell);
                RightSpell.OnUpdate(diff);
            }
        }

        private void UpdateState(SpellData theSpell)
        {
            var castingState = SpellHelper.GetCurrentCastingState(Actor, theSpell.Spell);
            DebugHelper.Print($"{theSpell.Spell.Name} state: {castingState}");
            switch (castingState)
            {
                case MagicCastingStates.Charged:
                case MagicCastingStates.Concentrating:
                    theSpell.State = SpellData.ChargingState.Charging;
                    break;
                case MagicCastingStates.Released:
                    if (theSpell.State == SpellData.ChargingState.Charging)
                        theSpell.State = SpellData.ChargingState.Released;
                    else
                        theSpell.State = SpellData.ChargingState.Cancelled;
                    break;
                case MagicCastingStates.None:
                    if (theSpell.State == SpellData.ChargingState.Charging)
                        theSpell.State = SpellData.ChargingState.Cancelled;
                    else
                        theSpell.State = SpellData.ChargingState.Idle;
                    break;
            }
        }

        public void OnSpendMagicka(SpellItem spell)
        {
            bool needsAssign;
            bool needsReset;
            var hand = SpellHelper.GetEquippedHand(Actor, spell);
            switch (hand)
            {
                case EquippedSpellSlots.LeftHand:
                    needsAssign = LeftSpell == null || LeftSpell.Spell != spell;
                    needsReset = LeftSpell != null && (needsAssign || LeftSpell.State != SpellData.ChargingState.Charging);

                    DebugHelper.Print($"Left Reset: {needsReset}, Assign: {needsAssign}");
                    if (needsReset)
                        LeftSpell.ResetSpellPower();
                    if (needsAssign)
                        LeftSpell = new SpellData(spell);
                    LeftSpell.State = SpellData.ChargingState.Charging;
                    return;
                case EquippedSpellSlots.RightHand:
                    needsAssign = RightSpell == null || RightSpell.Spell != spell;
                    needsReset = RightSpell != null && (needsAssign || RightSpell.State != SpellData.ChargingState.Charging);

                    DebugHelper.Print($"Left Reset: {needsReset}, Assign: {needsAssign}");
                    if (needsReset)
                        RightSpell.ResetSpellPower();
                    if (needsAssign)
                        RightSpell = new SpellData(spell);
                    RightSpell.State = SpellData.ChargingState.Charging;
                    return;
            }
        }
    }
}
