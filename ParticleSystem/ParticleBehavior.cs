using System;

namespace SpellChargingPlugin.ParticleSystem
{
    public abstract class ParticleBehavior
    {
        public bool Active { get; set; } = true;

        public abstract void Apply(Particle particle, float elapsedSeconds);
        public virtual void Reset() { }
        public virtual void Update(float elapsedSecondss) { }
    }
}