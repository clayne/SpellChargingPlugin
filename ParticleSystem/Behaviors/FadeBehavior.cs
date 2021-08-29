using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    public class FadeBehavior : ParticleBehavior
    {
        private float _duration;

        public FadeBehavior(float duration)
        {
            this._duration = duration;
        }

        public override void Apply(Particle particle, float elapsedSeconds)
        {
            particle.SetScale(particle.Object.LocalTransform.Scale - 0.337f * elapsedSeconds / _duration);
            if (particle.Object.LocalTransform.Scale <= 0)
                particle.Delete = true;
        }
    }
}
