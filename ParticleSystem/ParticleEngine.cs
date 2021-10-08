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
        public sealed class ParticleEngineCreationArgs
        {
            public int Limit { get; internal set; }
            public Func<IEnumerable<Particle>> ParticleBatchFactory { get; internal set; }
        }

        public int _particleLimit;
        private Func<IEnumerable<Particle>> _particlesFactory;
        private readonly HashSet<Particle> _activeParticles = new HashSet<Particle>();
        private readonly Utilities.SimpleTimer _cleanUpTimer = new Utilities.SimpleTimer();

        public void Clear()
        {
            foreach (var item in _activeParticles)
            {
                item.Dispose();
            }
            _activeParticles.Clear();
        }

        public void SpawnParticle()
        {
            if (_activeParticles.Count >= _particleLimit)
                return;
            foreach (var p in _particlesFactory())
                _activeParticles.Add(p);
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

        public static ParticleEngine Create(ParticleEngineCreationArgs args)
        {
            var ret = new ParticleEngine()
            {
                _particleLimit = args.Limit,
                _particlesFactory = args.ParticleBatchFactory,
            };
            return ret;
        }
    }
}
