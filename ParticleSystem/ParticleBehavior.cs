using System;

namespace SpellChargingPlugin.ParticleSystem
{
    public abstract class ParticleBehavior
    {
        public abstract void Apply(Particle particle, float elapsedSeconds);
    }
}