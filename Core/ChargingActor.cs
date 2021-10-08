using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using SpellChargingPlugin.StateMachine;
using SpellChargingPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.Core
{
    public class ChargingActor
    {
        public sealed class ChargingActorCreationArgs
        {
            public Character ParentCharacter { get; set; }
        }

        public Character Character { get; private set; }

        private ChargingSpell _chargingSpellLeft;
        private ChargingSpell _chargingSpellRight;
        private HotkeyPress _hotkeyEnableDisable;
        private bool _enabled;
        private Utilities.SimpleTimer _equippedSpellsCheckTimer;

        private ChargingActor() { }

        public static ChargingActor Create(ChargingActorCreationArgs args)
        {
            var ret = new ChargingActor
            {
                Character = args.ParentCharacter,
                _chargingSpellLeft = null,
                _chargingSpellRight = null,
                _enabled = true,
                _equippedSpellsCheckTimer = new Utilities.SimpleTimer(),
            };

            if (ret.Character.BaseForm.FormId == PlayerCharacter.Instance.BaseForm.FormId)
                ret.RegisterHotkey();
            return ret;
        }

        private void RegisterHotkey()
        {
            if (!HotkeyBase.TryParse(Settings.Instance.HotKey, out var keys))
                keys = new VirtualKeys[] { VirtualKeys.Shift, VirtualKeys.G };
            _hotkeyEnableDisable = new HotkeyPress(() =>
            {
                _enabled = !_enabled;
                if (_enabled)
                    return;
                _chargingSpellLeft?.Reset();
                _chargingSpellRight?.Reset();
            }, keys);
            _hotkeyEnableDisable.Register();
        }

        public void Update(float elapsedSeconds)
        {
            _equippedSpellsCheckTimer.Update(elapsedSeconds);
            if(_equippedSpellsCheckTimer.HasElapsed(1f, out var _))
                RefreshEquippedSpells();
            if (!_enabled)
                return;
            _chargingSpellLeft?.Update(elapsedSeconds);
            _chargingSpellRight?.Update(elapsedSeconds);
        }

        private void RefreshEquippedSpells()
        {
            var curLeftSpell = SpellHelper.GetSpell(Character, EquippedSpellSlots.LeftHand);
            if (curLeftSpell == null)
            {
                if (_chargingSpellLeft != null)
                {
                    _chargingSpellLeft.Reset();
                    _chargingSpellLeft = null;
                }
            }
            else
            {
                if(_chargingSpellLeft == null || curLeftSpell.FormId != _chargingSpellLeft.Spell.FormId)
                {
                    _chargingSpellLeft?.Reset();
                    _chargingSpellLeft = ChargingSpell.Create(new ChargingSpell.ChargingSpellCreationArgs() { Spell = curLeftSpell });
                }
            }

            var curRightSpell = SpellHelper.GetSpell(Character, EquippedSpellSlots.RightHand);
            if (curRightSpell == null)
            {
                if (_chargingSpellRight != null)
                {
                    _chargingSpellRight.Reset();
                    _chargingSpellRight = null;
                }
            }
            else
            {
                if (_chargingSpellRight == null || curRightSpell.FormId != _chargingSpellRight.Spell.FormId)
                {
                    _chargingSpellRight?.Reset();
                    _chargingSpellRight = ChargingSpell.Create(new ChargingSpell.ChargingSpellCreationArgs() { Spell = curRightSpell });
                }
            }
        }

        public bool TryConsumeMagicka(float magCost)
        {
            if (Character.GetActorValue(ActorValueIndices.Magicka) < magCost)
                return false;
            Character.DamageActorValue(ActorValueIndices.Magicka, -magCost);
            return true;
        }

        /// <summary>
        /// Check if the character is dual casting (same spell in both hands, dual casting perk, casting both)
        /// </summary>
        /// <returns></returns>
        public bool IsDualCasting()
            => Memory.InvokeThisCall(Character.Cast<Character>(), Utilities.Addresses.addr_ActorIsDualCasting).ToBool();
    }
}
