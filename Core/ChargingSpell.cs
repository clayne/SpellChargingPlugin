using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.Core
{
    public class ChargingSpell : IStateHolder<ChargingSpell>
    {
        public ChargingActor Holder { get; set; }
        public SpellItem Spell { get; set; }
        public EquippedSpellSlots Slot { get; set; }
        public State<ChargingSpell> CurrentState { get; set; }

        private PowerModifier[] _powerModifier;
        private SpellPower[] _spellBase;
        private int _chargeLevel;
        private NiAVObject _magicNode;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot)
        {
            Holder = holder;
            Spell = spell;
            Slot = slot;

            _spellBase = SpellHelper.DefineBaseSpellPower(Spell);
            _powerModifier = _spellBase.Select(_ => new PowerModifier(0f, 0)).ToArray();
            _chargeLevel = 0;

            var factory = new StateFactory<ChargingSpell>();
            var idleState = new States.Idle(factory, this);
            CurrentState = factory.GetOrCreate(() => idleState);
        }

        public void UpdateStats()
        {
            for (int i = 0; i < _spellBase.Length; i++)
            {
                var eff = Spell.Effects[i];

                eff.Magnitude = _spellBase[i].Magnitude + _powerModifier[i].Magnitude;
                eff.Duration = (int)(_spellBase[i].Duration + _powerModifier[i].Duration);
            }
        }

        public void Charge(float percentage)
        {
            var magCost = 100 * Settings.MagickaPercentagePerCharge;
            if (!HasMagickaForCharge(magCost))
                return;
            DrainMagicka(magCost);

            for (int i = 0; i < _spellBase.Length; i++)
            {
                var eff = Spell.Effects[i];

                bool hasMag = eff.Magnitude > 0f;
                bool hasDur = eff.Duration > 0;
                float realPercantage = percentage * (hasMag && hasDur ? 0.5f : 1f);

                _powerModifier[i].Magnitude += _spellBase[i].Magnitude * realPercantage;
                _powerModifier[i].Duration += _spellBase[i].Duration * realPercantage;

                DebugHelper.Print($"Charge {Spell.Name}.{eff.Effect.Name} bonusMAG: {_powerModifier[i].Magnitude}, bonusDur: {_powerModifier[i].Duration}");
            }
            ++_chargeLevel;
            UpdateStats();
            UpdateVisual();
        }

        private void DrainMagicka(float magCost)
        {
            Holder.Character.DamageActorValue(ActorValueIndices.Magicka, -magCost);
        }

        private bool HasMagickaForCharge(float magCost)
        {
            return Holder.Character.GetActorValue(ActorValueIndices.Magicka) >= magCost;
        }

        public void Reset()
        {
            for (int i = 0; i < Spell.Effects.Count; i++)
            {
                _powerModifier[i].Magnitude = 0f;
                _powerModifier[i].Duration = 0f;

                var eff = Spell.Effects[i];
                DebugHelper.Print($"Reset {Spell.Name}.{eff.Effect.Name}");
            }
            _chargeLevel = 0;
            UpdateStats();
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            // todo (NIOverride scale? audio? play cool effect?)

            // Get the player character instance.
            var plr = NetScriptFramework.SkyrimSE.PlayerCharacter.Instance;
            if (plr == null)
                return;

            // The player character root node.
            var plrRootNode = plr.Node;

            // This maybe possible if player is in loading screen or not in third person?
            if (plrRootNode == null)
                return;

            // Get the magic node node.
            if(_magicNode == null)
                _magicNode = Slot == EquippedSpellSlots.LeftHand 
                    ? plrRootNode.LookupNodeByName("NPC L MagicNode [LMag]")
                    : plrRootNode.LookupNodeByName("NPC R MagicNode [RMag]");
            if (_magicNode == null)
                return;

            _magicNode.LocalTransform.Scale = 1 + (_chargeLevel / 10f);
            DebugHelper.Print($"{_magicNode?.Name}.Scale = {_magicNode?.LocalTransform?.Scale}");
            _magicNode.Update(0.5f);
        }

    }
}
