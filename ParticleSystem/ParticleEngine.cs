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
        private readonly List<Particle> _activeParticles;
        private int _maxParticles;

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
            if(_activeParticles.Count < _maxParticles)
                _activeParticles.Add(newParticle);
        }

        public void Update(float elapsedSeconds)
        {
            if (!_activeParticles.Any())
                return;

            int i = 0;
            while (i < _activeParticles.Count)
            {
                var particle = _activeParticles[i];
                particle.Update(elapsedSeconds);

                if (particle.Delete)
                {
                    _activeParticles.RemoveAt(i);
                    particle.Dispose();
                    continue;
                }
                ++i;
            }
        }

        public static ParticleEngine Create(int maxParticles)
        {
            var ret = new ParticleEngine()
            {
                _maxParticles = maxParticles,
            };
            return ret;
        }
    }
}
