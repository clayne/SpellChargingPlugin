using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.IO;
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

            TransitionTo(() => new Idle(_context));
        }
    }
}
