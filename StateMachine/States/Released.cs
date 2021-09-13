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
        private readonly bool isDualCharge;

        public Released(ChargingSpell context, bool isDualCharge = false) : base(context)
        {
            this.isDualCharge = isDualCharge;
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var doMaintain = _context.Holder.IsHoldingKey && isDualCharge;
            var doShare = _context.Holder.IsHoldingKey && !doMaintain;

            if (doMaintain)
                _context.Holder.MaintainSpell(_context);

            SpellPowerManager.Instance.ApplyModifiers(_context);
            SpellPowerManager.Instance.ResetSpellModifiers(_context.Spell);

            if (doShare)
                _context.Holder.ShareSpell(_context.Spell, 1024f);

            Util.SimpleDeferredExecutor.Defer(
                () => SpellPowerManager.Instance.ResetSpellPower(_context.Spell),
                _context.Spell.FormId + 0xAFFE,
                Settings.Instance.AutoCleanupDelay,
                Settings.Instance.AutoCleanupDelay);

            TransitionTo(() => new Idle(_context));
        }
    }
}
