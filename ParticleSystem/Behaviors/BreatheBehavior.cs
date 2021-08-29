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
            public float OriginalScale, MinScale, MaxScale;
        }

        private readonly float minScale;
        private readonly float maxScale;
        private readonly float frequency;
        private Dictionary<Particle, BreatheState> _state = new Dictionary<Particle, BreatheState>();

        public BreatheBehavior(float minScale, float maxScale, float frequency)
        {
            this.minScale = minScale;
            this.maxScale = maxScale;
            this.frequency = frequency;
        }

        public override void Reset()
        {
            _state.Clear();
        }

        public override void Apply(Particle particle, float elapsedSeconds)
        {
            if (!_state.TryGetValue(particle, out BreatheState pState))
            {
                pState = new BreatheState()
                {
                    Expanding = Randomizer.NextInt(0, 100) > 50,
                    OriginalScale = particle.Object.LocalTransform.Scale,
                };
                pState.MinScale = pState.OriginalScale * minScale;
                pState.MaxScale = pState.OriginalScale * maxScale;
                _state.Add(particle, pState);
                //DebugHelper.Print($"Added P{particle.GetHashCode()}, Min = {pState.MinScale}, Max = {pState.MaxScale}, Orig = {pState.OriginalScale}, Expand = {pState.Expanding}");
            }
            float newScale;
            if (pState.Expanding)
            {
                var delta = pState.MaxScale - pState.OriginalScale;
                var toAdd = delta * elapsedSeconds * frequency;
                newScale = particle.Object.LocalTransform.Scale + toAdd;

                //DebugHelper.Print($"Expanding P{particle.GetHashCode()}, Delta = {delta}, Add = {toAdd}, New = {newScale}");

                if (newScale >= pState.MaxScale)
                {
                    //DebugHelper.Print($"P{particle.GetHashCode()}, Switching directions");
                    pState.Expanding = false;
                }
            }
            else
            {
                var delta = -(pState.OriginalScale - pState.MinScale);
                var toAdd = delta * elapsedSeconds * frequency;
                newScale = particle.Object.LocalTransform.Scale + toAdd;

                //DebugHelper.Print($"Contracting P{particle.GetHashCode()}, Delta = {delta}, Add = {toAdd}, New = {newScale}");

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
