using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Cancel : State<ChargingSpell>
    {
        public Cancel(ChargingSpell context) : base(context)
        {
        }

        /// <summary>
        /// Reset, clean up and transition to idle state
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        protected override void OnUpdate(float elapsedSeconds)
        {
            _context.Refund();
            _context.ResetAndClean();
            TransitionTo(() => new Idle(_context));
        }
    }
}
