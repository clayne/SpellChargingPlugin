using NetScriptFramework;
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
    public abstract class OverchargingBase : State<ChargingSpell>
    {
        protected readonly Util.SimpleTimer _chargingTimer = new Util.SimpleTimer();
        protected readonly float _inverseChargesPerSecond = 1f / Settings.Instance.ChargesPerSecond;

        public OverchargingBase(ChargingSpell context) : base(context)
        {
        }
    }
}
