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

        private Dictionary<ActiveEffect.EffectItem, SpellPower> _basePower;
        private Dictionary<ActiveEffect.EffectItem, PowerModifier> _powerModifier;
        private int _chargeLevel;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot)
        {
            Holder = holder;
            Spell = spell;
            Slot = slot;

            _basePower = new Dictionary<ActiveEffect.EffectItem, SpellPower>();
            _powerModifier = new Dictionary<ActiveEffect.EffectItem, PowerModifier>();
            _chargeLevel = 0;

            foreach (var eff in Spell.Effects)
            {
                _basePower.Add(eff, new SpellPower(eff.Magnitude, eff.Duration));
                _powerModifier.Add(eff, new PowerModifier(0f, 0));
            }

            var factory = new StateFactory<ChargingSpell>();
            var idleState = new States.Idle(factory, this);
            CurrentState = factory.GetOrCreate(() => idleState);
        }

        public void Update(float diff)
        {
            foreach (var eff in Spell.Effects)
            {
                eff.Magnitude = _basePower[eff].Magnitude + _powerModifier[eff].Magnitude;
                eff.Duration = _basePower[eff].Duration + _powerModifier[eff].Duration;
            }
        }

        public void Charge(float percentage)
        {
            var magCost = (int)(Spell.SpellData.CostOverride * percentage);
            if (!HasMagickaForCharge(magCost))
                return;
            DrainMagicka(magCost);

            foreach (var eff in Spell.Effects)
            {
                bool hasMag = eff.Magnitude > 0f;
                bool hasDur = eff.Duration > 0;
                float realPercantage = percentage * (hasMag && hasDur ? 0.5f : 1f);

                _powerModifier[eff].Magnitude += _basePower[eff].Magnitude * realPercantage;
                _powerModifier[eff].Duration += (int)(_basePower[eff].Duration * realPercantage);

                DebugHelper.Print($"Charge {Spell.Name}.{eff.Effect.Name} bMAG: {_powerModifier[eff].Magnitude}, bDur: {_powerModifier[eff].Duration}");
            }
            ++_chargeLevel;
            UpdateVisual();
        }

        private void DrainMagicka(int magCost)
        {
            Holder.Character.DamageActorValue(ActorValueIndices.Magicka, magCost);
        }

        private bool HasMagickaForCharge(int magCost)
        {
            return Holder.Character.GetActorValue(ActorValueIndices.Magicka) >= magCost;
        }

        public void Reset()
        {
            foreach (var eff in Spell.Effects)
            {
                _powerModifier[eff].Magnitude = _basePower[eff].Magnitude;
                _powerModifier[eff].Duration+= _basePower[eff].Duration;

                DebugHelper.Print($"Reset {Spell.Name}");
            }
            _chargeLevel = 0;
        }

        public void UpdateVisual()
        {
            // todo (NIOverride scale? audio? play cool effect?)
        }
    }
}
