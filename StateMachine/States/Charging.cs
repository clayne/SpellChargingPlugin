using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Charging : State<ChargingSpell>
    {
        private Utilities.SimpleTimer _preChargeControlTimer = new Utilities.SimpleTimer();

        public Charging(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetSpellAndState(_context.Owner.Character, _context.Slot);
            if (handState == null)
                return;

            switch (handState.Value.State)
            {
                case MagicCastingStates.Charging:
                    break;
                case MagicCastingStates.Charged:
                    _preChargeControlTimer.Update(elapsedSeconds);
                    if (!_preChargeControlTimer.HasElapsed(Settings.Instance.PreChargeDelay, out _))
                        return;
                    TransitionTo(() => new Overcharging(_context));
                    break;
                case MagicCastingStates.Released:
                    TransitionTo(() => new Released(_context));
                    break;
                default:
                    TransitionTo(() => new Canceled(_context));
                    break;
            }
        }
    }
}
