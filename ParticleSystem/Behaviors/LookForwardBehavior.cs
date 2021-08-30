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
    public class LookForwardBehavior : IParticleBehavior
    {
        public bool Active { get; set; }

        private NiPoint3 _distanceVec;
        private Particle _particle;

        public LookForwardBehavior(Particle particle)
        {
            _particle = particle;
            var alloc = Memory.Allocate(0x10);
            alloc.Pin();
            _distanceVec = MemoryObject.FromAddress<NiPoint3>(alloc.Address + 0x00);
            // TODO: make stuff actually track where the player is looking instead of doing this
            _distanceVec.X = 100f; _distanceVec.Y = -100f; _distanceVec.Z = 500f;
        }

        public void Update(float elapsedSeconds)
        {
            if (!Active)
                return;

            _particle.Object.LocalTransform.LookAt(_distanceVec);
        }
    }
}
