using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    public class FadeBehavior : IParticleBehavior
    {
        public bool Active { get; set; }

        private readonly Particle _particle;
        private readonly float _duration;

        public FadeBehavior(Particle particle, float duration)
        {
            _particle = particle;
            _duration = duration;
        }

        // TODO: does this even work as intended?
        public void Update(float elapsedSeconds)
        {
            if (!Active)
                return;

            _particle.SetScale(_particle.Object.LocalTransform.Scale - 1.1337f * elapsedSeconds / _duration);
            if (_particle.Object.LocalTransform.Scale <= 0)
                _particle.Delete = true;
        }
    }
}
