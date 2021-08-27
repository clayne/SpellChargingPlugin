using SpellChargingPlugin.Core;
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

        public Idle(StateFactory<ChargingSpell> factory, ChargingSpell context) : base(factory, context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetHandSpellState(_context.Holder.Character, _context.Slot);
            if (handState == null)
                return;
            if(_needsReset && _timeInState > 1.337f)
            {
                _context.Reset();
                _needsReset = false;
            }
            switch (handState.Value.State)
            {
                case NetScriptFramework.SkyrimSE.MagicCastingStates.Charged:
                case NetScriptFramework.SkyrimSE.MagicCastingStates.Concentrating: // does not work properly (yet)
                    if (_needsReset)
                    {
                        _context.Reset();
                        _needsReset = false;
                    }
                    TransitionTo(() => new Charging(_factory, _context));
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
