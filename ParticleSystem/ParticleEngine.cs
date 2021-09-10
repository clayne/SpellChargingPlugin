using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.ParticleSystem.Behaviors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem
{
    public sealed class ParticleEngine
    {
        public int ParticleCount => _activeParticles.Count;

        private readonly HashSet<Particle> _activeParticles = new HashSet<Particle>();
        private readonly Util.SimpleTimer _cleanUpTimer = new Util.SimpleTimer();

        public void Clear()
        {
            foreach (var item in _activeParticles)
            {
                item.Dispose();
            }
            _activeParticles.Clear();
        }

        public void Add(Particle newParticle)
        {
            _activeParticles.Add(newParticle);
        }

        public void Update(float elapsedSeconds)
        {
            if (_activeParticles.Count == 0)
                return;

            _cleanUpTimer.Update(elapsedSeconds);

            foreach (var item in _activeParticles.Where(p => !p.Delete))
            {
                item.Update(elapsedSeconds);
                if (item.Delete)
                    item.Dispose();
            }
            // This really doesn't need to happen all that often, I think
            if (_cleanUpTimer.HasElapsed(1.5f, out _))
                _activeParticles.RemoveWhere(e => e.Delete);
        }
    }
}
