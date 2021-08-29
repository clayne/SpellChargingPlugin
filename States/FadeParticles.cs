using SpellChargingPlugin.Core;
using SpellChargingPlugin.ParticleSystem.Behaviors;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.States
{
    public class FadeParticles : State<ChargingSpell>
    {
        public FadeParticles(StateFactory<ChargingSpell> factory, ChargingSpell context) : base(factory, context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            _context.ParticleEngine.Behaviors.Add(new FadeBehavior(1.0f));

            TransitionTo(() => new Idle(_factory, _context));
        }
    }
}
