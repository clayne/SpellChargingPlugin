using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.States
{
    public class Cancel : State<ChargingSpell>
    {
        public Cancel(StateFactory<ChargingSpell> factory, ChargingSpell context) : base(factory, context)
        {
        }

        /// <summary>
        /// Reset, clean up and transition to idle state
        /// </summary>
        /// <param name="diff"></param>
        public override void Update(float diff)
        {
            _context.Reset();
            TransitionTo(() => new Idle(_factory, _context));
        }
    }
}
