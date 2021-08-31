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
        public Character Actor => PlayerCharacter.Instance; // Currently only works on the player. Would be fun to see NPCs use this feature.
        public ChargingSpell LeftSpell { get; private set; }
        public ChargingSpell RightSpell { get; private set; }
        public TESCameraStates CameraState { get; private set; }

        bool _leftEqualsRight = false;

        public ChargingActor()
        {
            Register();
        }

        private void Register()
        {
            DebugHelper.Print($"[ChargingActor] Register OnUpdateCamera");
            Events.OnUpdateCamera.Register(e =>
            {
                var id = e.Camera.State.Id;
                if (id == CameraState)
                    return;
                CameraState = id;
                if (id == TESCameraStates.FirstPerson || id == TESCameraStates.ThirdPerson1 || id == TESCameraStates.ThirdPerson2)
                {
                    DebugHelper.Print($"[ChargingActor] Camera switch to {CameraState}. Expect funny visuals!");
                    LeftSpell?.UpdateParticleNode();
                    RightSpell?.UpdateParticleNode();
                }
            });
        }

        public void Update(float elapsedSeconds)
        {
            AssignSpellsIfNecessary();

            LeftSpell?.Update(elapsedSeconds);
            if (!_leftEqualsRight)
                RightSpell?.Update(elapsedSeconds);
        }

        /// <summary>
        /// Check if the character has spells equipped and assign them to their appropriate <cref>ChargingSpell</cref> slots.
        /// Also take care of cleaning up the previous <cref>ChargingSpell</cref> if overwriting.
        /// </summary>
        private void AssignSpellsIfNecessary()
        {
            var curLeft = SpellHelper.GetSpell(Actor, EquippedSpellSlots.LeftHand);

            if (curLeft == null)
            {
                LeftSpell = ClearSpell(LeftSpell);
                _leftEqualsRight = false;
            }
            else if (LeftSpell == null || LeftSpell.Spell.FormId != curLeft.FormId)
            {
                ClearSpell(LeftSpell);
                LeftSpell = SetSpell(curLeft, EquippedSpellSlots.LeftHand);
                _leftEqualsRight = LeftSpell.IsTwoHanded == true;
            }

            // Can skip right hand check and assignment when using master-tier or other two-handed spells.
            if (!_leftEqualsRight)
            {
                var curRight = SpellHelper.GetSpell(Actor, EquippedSpellSlots.RightHand);
                if (curRight == null)
                {
                    RightSpell = ClearSpell(RightSpell);
                }
                else if (RightSpell == null || RightSpell.Spell.FormId != curRight.FormId)
                {
                    ClearSpell(RightSpell);
                    RightSpell = SetSpell(curRight, EquippedSpellSlots.RightHand);
                }
            }
        }

        private ChargingSpell ClearSpell(ChargingSpell spell)
        {
            spell?.Reset();
            return null;
        }

        private ChargingSpell SetSpell(SpellItem spellItem, EquippedSpellSlots handSlot)
        {
            DebugHelper.Print($"[ChargingActor] Hand: {handSlot} -> {spellItem?.Name}");
            var ret = new ChargingSpell(this, spellItem, handSlot);
            return ret;
        }
    }
}
