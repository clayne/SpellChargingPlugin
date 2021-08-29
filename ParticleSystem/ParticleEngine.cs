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
        public bool Initialized { get; set; }
        public List<ParticleBehavior> Behaviors => _particleBehaviors;

        private readonly List<Particle> _activeParticles = new List<Particle>();
        private readonly List<ParticleBehavior> _particleBehaviors = new List<ParticleBehavior>();

        public Particle CreateFromNiAVObject(NiAVObject obj)
        {
            Particle ret = new Particle(obj);

            return ret;
        }

        public void ClearParticles()
        {
            foreach (var item in _activeParticles)
            {
                item.Dispose();
            }
            _activeParticles.Clear();
        }
        public void ResetBehaviors()
        {
            foreach (var behavior in _particleBehaviors)
            {
                behavior.Reset();
            }
        }

        public void Add(Particle newParticle)
        {
            _activeParticles.Add(newParticle);
        }

        public void Update(float elapsedSeconds)
        {
            if (!Initialized)
                return;
            if (!_activeParticles.Any())
                return;
            foreach (var behavior in _particleBehaviors)
            {
                if (!behavior.Active)
                    continue;
                behavior.Update(elapsedSeconds);
                //DebugHelper.Print($"Running particle behavior: {behavior}");
                int i = 0;
                while (i < _activeParticles.Count)
                {
                    var particle = _activeParticles[i];
                    if (particle.Delete)
                    {
                        _activeParticles.RemoveAt(i);
                        particle.Dispose();
                        continue;
                    }
                    behavior.Apply(particle, elapsedSeconds);
                    ++i;
                }
            }
        }

        public void AddBehavior(ParticleBehavior behavior)
        {
            _particleBehaviors.Add(behavior);
        }
    }
}
