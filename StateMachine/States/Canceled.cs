using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Canceled : State<ChargingSpell>
    {
        public Canceled(ChargingSpell context) : base(context)
        {
        }

        /// <summary>
        /// Reset, clean up and transition to idle state
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        protected override void OnUpdate(float elapsedSeconds)
        {
            if (_context.Spell.SpellData.CastingType != NetScriptFramework.SkyrimSE.EffectSettingCastingTypes.Concentration)
                _context.Refund();
            _context.Reset();
            TransitionTo(() => new Idle(_context));
        }
    }
}
