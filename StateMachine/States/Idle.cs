using NetScriptFramework;
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
            var handState = SpellHelper.GetSpellAndState(_context.Owner.Character, _context.Slot);
            if (handState == null)
                return;

            switch (handState.Value.State)
            {
                case MagicCastingStates.Concentrating:
                    if (!Settings.Instance.AllowConcentrationSpells)
                        break;
                    TransitionTo(() => new OverConcentrating(_context));
                    break;
                case MagicCastingStates.Charging:
                    TransitionTo(() => new Charging(_context));
                    break;
            }
        }
    }
}
