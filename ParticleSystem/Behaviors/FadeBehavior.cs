using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    public class FadeBehavior : IParticleBehavior
    {
        public Func<bool> Active { get; set; } = () => true;

        private readonly Particle _particle;
        private readonly float _delta;

        public FadeBehavior(Particle particle, float maxDuration)
        {
            _particle = particle;
            _delta = particle.Object.LocalTransform.Scale / maxDuration;
        }

        // TODO: does this even work as intended?
        public void Update(float elapsedSeconds)
        {
            if (!Active())
                return;

            if (_particle.Delete)
                return;

            var newScale = _particle.Object.LocalTransform.Scale - _delta * elapsedSeconds;
            _particle.SetScale(newScale);
            if (_particle.Object.LocalTransform.Scale <= 0)
                _particle.Delete = true;
        }
    }
}
