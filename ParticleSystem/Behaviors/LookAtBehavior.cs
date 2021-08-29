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
        private readonly Character _character;
        private NiPoint3 _distanceVec;

        public LookAtBehavior(Character character)
        {
            var alloc = Memory.Allocate(0x10);
            alloc.Pin();
            _distanceVec = MemoryObject.FromAddress<NiPoint3>(alloc.Address + 0x00);
            _distanceVec.X = 100f; _distanceVec.Y = -100f; _distanceVec.Z = 500f;
            this._character = character;
        }

        public override void Apply(Particle particle, float elapsedSeconds)
        {
            particle.Object.LocalTransform.LookAt(_distanceVec);
        }
    }
}
