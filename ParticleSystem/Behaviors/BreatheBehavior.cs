using NetScriptFramework.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    public class BreatheBehavior : IParticleBehavior
    {
        public bool Active { get; set; }

        private readonly float _originalScale;
        private readonly float _minScale;
        private readonly float _maxScale;
        private readonly float _scaleDelta;
        private readonly Particle _particle;
        private bool _isExpanding;

        public BreatheBehavior(Particle particle, float minScaleFactor, float maxScaleFactor, float frequency)
        {
            _originalScale = particle.Object.LocalTransform.Scale;
            _minScale = _originalScale * minScaleFactor;
            _maxScale = _originalScale * maxScaleFactor;
            _scaleDelta = (_originalScale * _maxScale - _originalScale * _minScale) * frequency;
            _isExpanding = Randomizer.NextInt(0, 100) > 50;
        }

        public void Update(float elapsedSeconds)
        {
            if (!Active)
                return;

            float newScale;
            if (_isExpanding)
            {
                newScale = _particle.Object.LocalTransform.Scale + _scaleDelta * elapsedSeconds;
                _isExpanding = newScale > _maxScale;
            }
            else
            {
                newScale = _particle.Object.LocalTransform.Scale - _scaleDelta * elapsedSeconds;
                _isExpanding = newScale <= _minScale;
            }
            _particle.Object.LocalTransform.Scale = newScale;
        }

    }
}
