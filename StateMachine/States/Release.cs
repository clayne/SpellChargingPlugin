using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Release : State<ChargingSpell>
    {
        public Release(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            // Task.Run(async () => { await Task.Delay(3000); _context.ResetAndClean(); });
            TransitionTo(() => new Idle(_context));
        }
    }
}
