using NetScriptFramework.SkyrimSE;
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
            // TODO: move reset & cleanup to separate behavior or something
            var handState = SpellHelper.GetSpellAndState(_context.Holder.Actor, _context.Slot);
            if (handState == null)
            {
                if (_needsReset) { _context.Reset(); _needsReset = false; }
                return;
            }
            if(_needsReset && _timeInState > 5f)
            {
                DebugHelper.Print($"[State.Idle] state auto-cleanup {_context.Spell.Name}");
                _context.Reset();
                _needsReset = false;
            }
            switch (handState.Value.State)
            {
                case MagicCastingStates.Charged:
                case MagicCastingStates.Concentrating: // does not work properly (yet)
                    if (!Settings.Instance.AllowConcentrationSpells && handState.Value.State == MagicCastingStates.Concentrating)
                        break;
                    
                    if (_needsReset)
                    {
                        _context.Reset();
                        _needsReset = false;
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
