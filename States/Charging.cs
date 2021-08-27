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
        /// <param name="elapsedSeconds"></param>
        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetHandSpellState(_context.Holder.Character, _context.Slot);
            
            switch (handState?.State)
            {
                case NetScriptFramework.SkyrimSE.MagicCastingStates.Released:
                    this.TransitionTo(() => new Release(_factory, _context));
                    break;
                case NetScriptFramework.SkyrimSE.MagicCastingStates.None:
                case null:
                    this.TransitionTo(() => new Cancel(_factory, _context));
                    break;
                default:
                    _chargeTime += elapsedSeconds;
                    if (_chargeTime >= Settings.ChargeInterval)
                    {
                        _chargeTime = 0.0f;
                        _context.Charge(Settings.ChargeIncrement);
                    }
                    break;
            }
        }
    }
}
