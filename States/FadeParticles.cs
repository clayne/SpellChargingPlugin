using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.States
{
    public class FadeParticles : State<ChargingSpell>
    {
        public FadeParticles(StateFactory<ChargingSpell> factory, ChargingSpell context) : base(factory, context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            // do cleanup

            //if (_needFadeAway == false || _fadeBuffer == null)
            //{
            //    IsResetting = false;
            //    return;
            //}
            //if (elapsedSeconds <= 0f)
            //    return;

            //var particlesLeft = _fadeBuffer.Count - _fadeTaken;
            //int takeThisFrame = Math.Max(1, (int)(elapsedSeconds * _fadePerSec));
            //var skip = particlesLeft > takeThisFrame ? 1 : 0;

            //DebugHelper.Print($"Fade items left: {particlesLeft}, taking {takeThisFrame} this update (skip {skip + _fadeTaken})");

            //foreach (var item in _fadeBuffer.Skip(skip + _fadeTaken).Take(takeThisFrame))
            //{
            //    var particle = item.Item1;
            //    DeleteFromWorld(particle);
            //}

            //_fadeTaken += takeThisFrame;

            //if (skip == 0)
            //{
            //    DebugHelper.Print($"Nothing left to fade. Clear");
            //    _fadeBuffer.Clear();
            //    _fadeBuffer = null;
            //    _needFadeAway = false;
            //    _fadeTaken = 0;
            //    IsResetting = false;
            //}


            //void DeleteFromWorld(NiAVObject particle)
            //{
            //    particle?.Detach();
            //    particle?.DecRef();
            //}

            TransitionTo(() => new Idle(_factory, _context));
        }


    }
}
