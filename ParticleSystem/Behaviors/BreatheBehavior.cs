using NetScriptFramework.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    public class BreatheBehavior : ParticleBehavior
    {
        private class BreatheState
        {
            public bool Expanding;
            public float OriginalScale, MinScale, MaxScale, Delta;
        }

        private readonly float _minScale;
        private readonly float _maxScale;
        private readonly float _frequency;
        private Dictionary<int, BreatheState> _states = new Dictionary<int, BreatheState>();

        public BreatheBehavior(float minScale, float maxScale, float frequency)
        {
            this._minScale = minScale;
            this._maxScale = maxScale;
            this._frequency = frequency;
        }

        public override void Reset()
        {
            _states.Clear();
        }

        public override void Apply(Particle particle, float elapsedSeconds)
        {
            if (!_states.TryGetValue(particle.GetHashCode(), out BreatheState pState))
            {
                pState = new BreatheState()
                {
                    Expanding = Randomizer.NextInt(0, 100) > 50,
                    OriginalScale = particle.Object.LocalTransform.Scale,
                };
                pState.MinScale = pState.OriginalScale * _minScale;
                pState.MaxScale = pState.OriginalScale * _maxScale;
                pState.Delta = pState.OriginalScale * pState.MaxScale - pState.OriginalScale * pState.MinScale;
                pState.Delta *= _frequency;
                _states.Add(particle.GetHashCode(), pState);
                //DebugHelper.Print($"Added P{particle.GetHashCode()}, Min = {pState.MinScale}, Max = {pState.MaxScale}, Orig = {pState.OriginalScale}, Expand = {pState.Expanding}");
            }
            float newScale;
            if (pState.Expanding)
            {
                newScale = particle.Object.LocalTransform.Scale + pState.Delta * elapsedSeconds;
                //DebugHelper.Print($"Expanding P{particle.GetHashCode()}, Delta = {pState.Delta}, New = {newScale}");

                if (newScale >= pState.MaxScale)
                {
                    //DebugHelper.Print($"P{particle.GetHashCode()}, Switching directions");
                    pState.Expanding = false;
                }
            }
            else
            {
                newScale = particle.Object.LocalTransform.Scale - pState.Delta * elapsedSeconds;
                //DebugHelper.Print($"Contracting P{particle.GetHashCode()}, Delta = {pState.Delta}, New = {newScale}");

                if (newScale <= pState.MinScale)
                {
                    //DebugHelper.Print($"P{particle.GetHashCode()}, Switching directions");
                    pState.Expanding = true;
                }
            }
            particle.Object.LocalTransform.Scale = newScale;
        }

    }
}
