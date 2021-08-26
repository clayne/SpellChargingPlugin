using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.States
{
    public class Charging : State<ChargingSpell>
    {
        float _chargeTime = 0.0f;
        public Charging(StateFactory<ChargingSpell> factory, ChargingSpell context) : base(factory, context)
        {
        }

        /// <summary>
        /// Increase magnitude, transition to Released or Idle states if neccessary
        /// </summary>
        /// <param name="diff"></param>
        public override void Update(float diff)
        {
            var castState = SpellHelper.GetCurrentCastingState(_context.Holder.Character, _context.Spell);
            switch (castState)
            {
                case NetScriptFramework.SkyrimSE.MagicCastingStates.Released:
                    this.TransitionTo(() => new Release(_factory, _context));
                    break;
                case NetScriptFramework.SkyrimSE.MagicCastingStates.None:
                    this.TransitionTo(() => new Cancel(_factory, _context));
                    break;
                default:
                    _chargeTime += diff;
                    if (_chargeTime >= Settings.ChargeInterval)
                    {
                        _chargeTime = 0.0f;
                        _context.Charge(Settings.ChargeIncrement);
                        _context.Update(diff);
                    }
                    break;
            }
        }
    }
}
