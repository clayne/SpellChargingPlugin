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
        public Idle(StateFactory<ChargingSpell> factory, ChargingSpell context) : base(factory, context)
        {
        }

        public override void Update(float diff)
        {
            var castState = SpellHelper.GetCurrentCastingState(_context.Holder.Character, _context.Spell);
            switch (castState)
            {
                case NetScriptFramework.SkyrimSE.MagicCastingStates.Charged:
                case NetScriptFramework.SkyrimSE.MagicCastingStates.Concentrating:
                    TransitionTo(() => new Charging(_factory, _context));
                    break;
            }
        }
    }
}
