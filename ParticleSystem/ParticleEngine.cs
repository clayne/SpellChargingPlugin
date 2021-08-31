using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.ParticleSystem.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.ParticleSystem
{
    public partial class ParticleEngine
    {
        public static uint GlobalParticleCount { get; set; }

        private readonly List<Particle> _activeParticles = new List<Particle>();
        private uint _maxParticles;
        

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
        }

        private float __timer = 0f;
        public void Update(float elapsedSeconds)
        {
            if (!_activeParticles.Any())
                return;

            if (Settings.Instance.LogDebugMessages)
            {
                __timer += elapsedSeconds;
                if(__timer > 5.0f)
                {
                    DebugHelper.Print($"[ParticleEngine] #{this.GetHashCode():X} Particles: {_activeParticles.Count} of global total {GlobalParticleCount}");
                    __timer = 0.0f;
                }
            }

            int i = 0;
            while (i < _activeParticles.Count)
            {
                var particle = _activeParticles[i];
                particle.Update(elapsedSeconds);

                if (particle.Delete)
                {
                    _activeParticles.RemoveAt(i);
                    particle.Dispose();
                    --i;
                }
                ++i;
            }
        }

        public static ParticleEngine Create(uint maxParticles)
        {
            var ret = new ParticleEngine()
            {
                _maxParticles = maxParticles,
            };
            return ret;
        }
    }
}
