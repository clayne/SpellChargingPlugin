using SpellChargingPlugin.Core;
using SpellChargingPlugin.ParticleSystem.Behaviors;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.States
{
    public class Idle : State<ChargingSpell>
    {
        private bool _needsReset = false;

        public Idle(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetSpellAndState(_context.Holder.Actor, _context.Slot);
            if (handState == null)
            {
                if (_needsReset) { _context.Reset(); _needsReset = false; }
                return;
            }
            if(_needsReset && _timeInState > 10f)
            {
                DebugHelper.Print($"Idle state auto-cleanup {_context.Spell.Name}");
                _context.Reset();
                _needsReset = false;
            }
            switch (handState.Value.State)
            {
                case NetScriptFramework.SkyrimSE.MagicCastingStates.Charged:
                //case NetScriptFramework.SkyrimSE.MagicCastingStates.Concentrating: // does not work properly (yet)
                // TODO: move reset & cleanup to separate behavior or something
                    if (_needsReset)
                    {
                        _context.Reset();
                        _needsReset = false;
                    }
                    var peb = _context.Particle.Behaviors;
                    var fadeBehavior = peb.OfType<FadeBehavior>();
                    foreach (var item in fadeBehavior.ToList())
                    {
                        peb.Remove(item);
                    }

                    TransitionTo(() => new Charging(_context));
                    break;
            }
        }

        protected override void OnEnterState()
        {
            _needsReset = true;
            base.OnEnterState();
        }
    }
}
