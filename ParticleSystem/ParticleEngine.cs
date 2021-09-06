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
    public partial class ParticleEngine
    {
        public static uint GlobalParticleCount { get; set; }

        private HashSet<Particle> _activeParticles = new HashSet<Particle>();
        private uint _maxParticles;
        private Util.SimpleTimer _cleanUpTimer = new Util.SimpleTimer();

        private ParticleEngine() { }
        public static ParticleEngine Create(uint maxParticles)
        {
            var ret = new ParticleEngine()
            {
                _maxParticles = maxParticles,
            };
            return ret;
        }

        public void Clear()
        {
            foreach (var item in _activeParticles)
            {
                item.Dispose();
            }
            _activeParticles.Clear();
            DebugHelper.Print($"[ParticleEngine:{GetHashCode()}] Cleared. {GlobalParticleCount} active particles remain.");
        }

        public void Add(Particle newParticle)
        {
            if (_activeParticles.Count >= _maxParticles)
                return;
            _activeParticles.Add(newParticle);
            ++GlobalParticleCount;
            if (GlobalParticleCount % 25 == 0)
                DebugHelper.Print($"[ParticleEngine] Total particles: {GlobalParticleCount}");
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
            if (_cleanUpTimer.HasElapsed(0.5f, out _))
                _activeParticles.RemoveWhere(e => e.Delete);
        }
    }
}
