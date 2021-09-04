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
        private Util.SimpleTimer _sTimer = new Util.SimpleTimer();

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
            if (GlobalParticleCount != 0)
                DebugHelper.Print($"[ParticleEngine] Not all particles reset?");
        }

        public void Add(Particle newParticle)
        {
            if (_activeParticles.Count >= _maxParticles)
                return;
            _activeParticles.Add(newParticle);
            ++GlobalParticleCount;
            if (GlobalParticleCount % 50 == 0)
                DebugHelper.Print($"[ParticleEngine] Total particles: {GlobalParticleCount}");
        }

        public void Update(float elapsedSeconds)
        {
            if (_activeParticles.Count == 0)
                return;

            _sTimer.Update(elapsedSeconds);

            foreach (var item in _activeParticles.Where(p => !p.Delete))
            {
                item.Update(elapsedSeconds);
                if (item.Delete)
                    item.Dispose();
            }

            // This really doesn't need to happen all that often, I think
            if(_sTimer.HasElapsed(0.33f, out _))
                _activeParticles.RemoveWhere(e => e.Delete);
        }
    }
}
