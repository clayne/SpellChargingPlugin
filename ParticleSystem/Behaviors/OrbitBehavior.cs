using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    public class OrbitBehavior : ParticleBehavior
    {
        private readonly NiPoint3 _orbitCenter;

        public OrbitBehavior(NiPoint3 center)
        {
            this._orbitCenter = center;
        }
        public override void Apply(Particle particle, float elapsedSeconds)
        {
            particle.Rotate(_orbitCenter, particle.Velocity, 180f * elapsedSeconds);
        }
    }
}
