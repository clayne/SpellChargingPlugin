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

        private ParallelOptions _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 2 };

        private ConcurrentBag<Particle> _activeParticles = new ConcurrentBag<Particle>();
        private uint _maxParticles;

        private readonly float _fMaxUpdateTime = 1f / Settings.Instance.UpdatesPerSecond;
        private Util.SimpleAverager _averageUtil = new Util.SimpleAverager(Settings.Instance.UpdatesPerSecond);

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
            while (!_activeParticles.IsEmpty)
                _activeParticles.TryTake(out var _);
            if (GlobalParticleCount != 0)
                DebugHelper.Print($"[ParticleEngine] Not all particles reset?");
        }

        public void Add(Particle newParticle)
        {
            if (_activeParticles.Count >= _maxParticles)
                return;
            _activeParticles.Add(newParticle);
            ++GlobalParticleCount;
            if (GlobalParticleCount % 100 == 0)
                DebugHelper.Print($"[ParticleEngine] Total particles: {GlobalParticleCount}");
        }

        public void Update(float elapsedSeconds)
        {
            if (!_activeParticles.Any())
                return;

            UpdateSingleThreaded(elapsedSeconds);

            if (_activeParticles.Any(p => p.Delete))
                DeleteParticles();
        }

        private void UpdateSingleThreaded(float elapsedSeconds)
        {
            foreach (var p in _activeParticles)
                p.Update(elapsedSeconds);
        }

        // TODO: check performance
        private void DeleteParticles()
        {
            var toRemove = _activeParticles.Where(p => p.Delete);
            foreach (var p in toRemove)
                p.Dispose();
            _activeParticles = new ConcurrentBag<Particle>(_activeParticles.Where(p => !p.Delete));
        }
    }
}
