using System;

namespace SpellChargingPlugin.ParticleSystem
{
    public interface IParticleBehavior
    {
        bool Active { get; set; }
        void Update(float elapsedSeconds);
    }
}