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
        Dictionary<ActiveEffect.EffectItem, SpellPower> BasePower { get; set; }
        Dictionary<ActiveEffect.EffectItem, PowerModifier>  Modifier { get; set; }
        public State<ChargingSpell> PreviousState { get; set; }
        public State<ChargingSpell> CurrentState { get; set; }

        public ChargingSpell(ChargingActor holder, SpellItem spell)
        {
            Holder = holder;
            Spell = spell;
            BasePower = new Dictionary<ActiveEffect.EffectItem, SpellPower>();
            Modifier = new Dictionary<ActiveEffect.EffectItem, PowerModifier>();
            foreach (var eff in Spell.Effects)
            {
                BasePower.Add(eff, new SpellPower(eff.Magnitude, eff.Duration));
                Modifier.Add(eff, new PowerModifier(0f, 0));
            }

            var factory = new StateFactory<ChargingSpell>();
            var idleState = new States.Idle(factory, this);
            CurrentState = factory.GetOrCreate(() => idleState);
        }

        public void Update(float diff)
        {
            foreach (var eff in Spell.Effects)
            {
                eff.Magnitude = BasePower[eff].Magnitude + Modifier[eff].Magnitude;
                eff.Duration = BasePower[eff].Duration + Modifier[eff].Duration;
            }
        }

        public void Charge(float percentage)
        {
            foreach (var eff in Spell.Effects)
            {
                bool hasMag = eff.Magnitude > 0f;
                bool hasDur = eff.Duration > 0;
                float realPercantage = percentage * (hasMag && hasDur ? 0.5f : 1f);

                Modifier[eff].Magnitude += BasePower[eff].Magnitude * realPercantage;
                Modifier[eff].Duration += (int)(BasePower[eff].Duration * realPercantage);

                DebugHelper.Print($"Charge {Spell.Name}.{eff.Effect.Name} bMAG: {Modifier[eff].Magnitude}, bDur: {Modifier[eff].Duration}");
            }
        }

        public void Reset()
        {
            foreach (var eff in Spell.Effects)
            {
                Modifier[eff].Magnitude = BasePower[eff].Magnitude;
                Modifier[eff].Duration+= BasePower[eff].Duration;

                DebugHelper.Print($"Reset {Spell.Name}");
            }
        }
    }
}
