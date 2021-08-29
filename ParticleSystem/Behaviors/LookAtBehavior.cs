using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem.Behaviors
{
    public class LookAtBehavior : ParticleBehavior
    {
        private NiPoint3 _distanceVec;

        public LookAtBehavior()
        {
            var alloc = Memory.Allocate(0x10);
            alloc.Pin();
            _distanceVec = MemoryObject.FromAddress<NiPoint3>(alloc.Address + 0x00);
            _distanceVec.X = 0f; _distanceVec.Y = -10000f; _distanceVec.Z = 10000f;
        }

        public override void Apply(Particle particle, float elapsedSeconds)
        {
            particle.Object.LocalTransform.LookAt(_distanceVec);
        }
    }
}
