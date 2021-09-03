using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Charging : State<ChargingSpell>
    {
        private static int _chargingInstances = 0;
        public Charging(ChargingSpell context) : base(context)
        {
        }

        /// <summary>
        /// Increase magnitude, transition to Released or Idle states if neccessary
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetSpellAndState(_context.Holder.Actor, _context.Slot);
            
            switch (handState?.State)
            {
                case MagicCastingStates.Charging:
                    break;
                case MagicCastingStates.Charged:
                case MagicCastingStates.Concentrating:
                    _context.UpdateCharge(elapsedSeconds);
                    break;
                case MagicCastingStates.Released:
                    TransitionTo(() => new Release(_context));
                    break;
                case MagicCastingStates.None:
                case null:
                default:
                    TransitionTo(() => new Cancel(_context));
                    break;
            }
        }

        // These SHOULD be enough to track whether or not the player is still firing spells and keep the "victims" inside
        // ActiveEffectTracker alive until the player stops, at which point there SHOULD be no more ActiveEffects to update
        // and the cache can be thrown away
        protected override void OnEnterState()
        {
            ++_chargingInstances;
        }
        protected override void OnExitState()
        {
            if (--_chargingInstances == 0)
                ActiveEffectTracker.Instance.Clear();
        }
    }
}
