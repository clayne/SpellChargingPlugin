using System;

namespace SpellChargingPlugin.ParticleSystem
{
    public interface IParticleBehavior
    {
        Func<bool> Active { get; set; }
        void Update(float elapsedSeconds);
    }
}