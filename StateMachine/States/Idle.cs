using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;
using SpellChargingPlugin.ParticleSystem.Behaviors;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Idle : State<ChargingSpell>
    {
        public Idle(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetSpellAndState(_context.Holder.Actor, _context.Slot);
            if (handState == null)
                return;
            switch (handState.Value.State)
            {
                case MagicCastingStates.Concentrating:
                    if (!Settings.Instance.AllowConcentrationSpells)
                        break;
                    goto case MagicCastingStates.Charged;
                case MagicCastingStates.Charged:
                    _context.ResetAndClean();
                    TransitionTo(() => new Charging(_context));
                    break;
            }
        }
    }
}
