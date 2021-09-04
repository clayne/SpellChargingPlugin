using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
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
        public enum OperationMode { Disabled, Magnitude, Duration, Both }
        readonly uint[] _modeArtObjects = { 0x00, 0xE7559, 0xE7559, 0xE7559 }; // find or create some decent visuals
        public Character Actor { get; }
        public OperationMode Mode { get; private set; } = OperationMode.Disabled;

        private ChargingSpell _chargingSpellLeft;
        private ChargingSpell _chargingSpellRight;
        private TESCameraStates _cameraState;
        private bool _leftEqualsRight = false;

        public ChargingActor(Character character)
        {
            Actor = character;
            Register();
            if (Enum.TryParse<OperationMode>(Settings.Instance.OperationMode, out var mode))
                SetOperationMode(mode);
            else
                SetOperationMode(OperationMode.Magnitude);
        }

        /// <summary>
        /// Do all event registrations here
        /// </summary>
        private void Register()
        {
            DebugHelper.Print($"[ChargingActor] Register OnUpdateCamera");
            Events.OnUpdateCamera.Register(e =>
            {
                var id = e.Camera.State.Id;
                if (id == _cameraState)
                    return;
                _cameraState = id;
                if (id == TESCameraStates.FirstPerson || id == TESCameraStates.ThirdPerson1 || id == TESCameraStates.ThirdPerson2)
                {
                    DebugHelper.Print($"[ChargingActor] Camera switch to {_cameraState}. Expect funny visuals!");
                    _chargingSpellLeft?.RefreshParticleNode();
                    _chargingSpellRight?.RefreshParticleNode();
                }
            });

            if (!HotkeyBase.TryParse(Settings.Instance.HotKey, out var keys))
                keys = new VirtualKeys[] { VirtualKeys.Shift, VirtualKeys.G };
            var keyPress = new HotkeyPress(() => RotateOperationMode(), keys);
            keyPress.Register();

            // Having something like this would be nice
            //Events.OnEquipWeaponOrSpell.Register(arg => { });
        }

        private void RotateOperationMode()
        {
            int cur = (int)Mode;
            int next = (++cur) % 4;
            OperationMode nextMode = (OperationMode)next;
            SetOperationMode(nextMode);
        }

        /// <summary>
        /// Switch between modes or disable altogether
        /// </summary>
        private void SetOperationMode(OperationMode newMode)
        {
            foreach (var fid in _modeArtObjects)
                Util.Visuals.DetachArtObject(fid, Actor);

            MenuManager.ShowHUDMessage($"Overcharge : {newMode}", null, false);

            if (Mode == OperationMode.Disabled && newMode != OperationMode.Disabled)
                Util.Visuals.AttachArtObject(0x56AC8, Actor); // ShieldSpellFX
            if (newMode != OperationMode.Disabled)
                Util.Visuals.AttachArtObject(_modeArtObjects[(int)newMode], Actor);
            else // newMode == disabled
            {
                Util.Visuals.DetachArtObject(0x56AC8, Actor); // ShieldSpellFX
                ClearSpell(ref _chargingSpellLeft);
                ClearSpell(ref _chargingSpellRight);
            }

            Mode = newMode;
        }

        /// <summary>
        /// Check for changes in equipped spells and refresh their states
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        public void Update(float elapsedSeconds)
        {
            HotkeyBase.UpdateAll();
            if (Mode == OperationMode.Disabled)
                return;

            AssignSpellsIfNecessary();

            _chargingSpellLeft?.Update(elapsedSeconds);
            if (!_leftEqualsRight)
                _chargingSpellRight?.Update(elapsedSeconds);
        }

        /// <summary>
        /// Check if the character has spells equipped and assign them to their appropriate <cref>ChargingSpell</cref> slots.
        /// Also take care of cleaning up the previous <cref>ChargingSpell</cref> if overwriting.
        /// </summary>
        private void AssignSpellsIfNecessary()
        {
            var actualLeftSpell = SpellHelper.GetSpell(Actor, EquippedSpellSlots.LeftHand);

            if (actualLeftSpell == null)
            {
                ClearSpell(ref _chargingSpellLeft);
                _leftEqualsRight = false;
            }
            else if (_chargingSpellLeft == null || _chargingSpellLeft.Spell.FormId != actualLeftSpell.FormId)
            {
                ClearSpell(ref _chargingSpellLeft);
                _chargingSpellLeft = SetSpell(actualLeftSpell, EquippedSpellSlots.LeftHand);
                _leftEqualsRight = _chargingSpellLeft.IsTwoHanded == true;
            }

            // Can skip right hand check and assignment when using master-tier or other two-handed spells.
            if (!_leftEqualsRight)
            {
                var actualRightSpell = SpellHelper.GetSpell(Actor, EquippedSpellSlots.RightHand);
                if (actualRightSpell == null)
                {
                    ClearSpell(ref _chargingSpellRight);
                }
                else if (_chargingSpellRight == null || _chargingSpellRight.Spell.FormId != actualRightSpell.FormId)
                {
                    ClearSpell(ref _chargingSpellRight);
                    _chargingSpellRight = SetSpell(actualRightSpell, EquippedSpellSlots.RightHand);
                }
            }
        }

        /// <summary>
        /// Reset the charging spell null the reference
        /// </summary>
        /// <param name="spell"></param>
        private void ClearSpell(ref ChargingSpell spell)
        {
            spell?.ResetAndClean();
            spell = null;
        }

        /// <summary>
        /// Create a new charging spell
        /// </summary>
        /// <param name="spellItem"></param>
        /// <param name="handSlot"></param>
        /// <returns></returns>
        private ChargingSpell SetSpell(SpellItem spellItem, EquippedSpellSlots handSlot)
        {
            DebugHelper.Print($"[ChargingActor] Hand: {handSlot} -> {spellItem?.Name}");
            return new ChargingSpell(this, spellItem, handSlot);
        }
    }
}
