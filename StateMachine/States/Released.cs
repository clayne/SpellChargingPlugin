using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Released : State<ChargingSpell>
    {
        public Released(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            SpellPowerManager.Instance.ApplyModifiers(_context);
            SpellPowerManager.Instance.ResetSpellModifiers(_context.Spell);
            SpellPowerManager.Instance.RegisterForReset(_context);

            Util.SimpleDeferredExecutor.Defer(() => SpellPowerManager.Instance.ResetSpellPower(_context.Spell), _context.Spell.FormId + 0xAFFE, Settings.Instance.AutoCleanupDelay);

            TransitionTo(() => new Idle(_context));
        }
    }
}
