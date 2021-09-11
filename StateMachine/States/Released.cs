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
        private static SpellItem _maintainSpell = null;
        private readonly bool isDualCharge;

        static Released()
        {
            var cac = CachedFormList.TryParse("801:SpellChargingPlugin.esp", "Overcharge", "EffectForm");
            if (cac == null || cac.All.Count != 1)
                return;
            _maintainSpell = cac.All[0] as SpellItem;
        }


        public Released(ChargingSpell context, bool isDualCharge = false) : base(context)
        {
            this.isDualCharge = isDualCharge;
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var doMaintain = _context.Holder.IsHoldingKey && isDualCharge;
            var doShare = _context.Holder.IsHoldingKey && !doMaintain;

            if (doMaintain)
                Maintain();

            SpellPowerManager.Instance.ApplyModifiers(_context);
            SpellPowerManager.Instance.ResetSpellModifiers(_context.Spell);

            if (doShare)
                Share(1024f);

            Util.SimpleDeferredExecutor.Defer(
                () => SpellPowerManager.Instance.ResetSpellPower(_context.Spell), 
                _context.Spell.FormId + 0xAFFE, 
                Settings.Instance.AutoCleanupDelay,
                Settings.Instance.AutoCleanupDelay);

            TransitionTo(() => new Idle(_context));
        }

        /// <summary>
        /// Apply spell effect to all nearby allies
        /// </summary>
        /// <param name="range"></param>
        private void Share(float range)
        {
            var inRange = Util.GetCharactersInRange(_context.Holder.Actor, range);
            foreach (var ally in inRange.Where(chr => !chr.IsDead && chr.IsPlayerTeammate && !chr.IsPlayer))
                ally.CastSpell(_context.Spell, ally, null);
            MenuManager.ShowHUDMessage($"Share : {_context.Spell.Name}", null, false);
        }

        /// <summary>
        /// Maintain the spell permanently at a cost
        /// </summary>
        private unsafe void Maintain()
        {
            if (_maintainSpell == null)
                return;
            var spell = _context.Spell;
            var actor = _context.Holder.Actor;

            var maintainCost = _context.ChargeLevel * Settings.Instance.MagickaPerCharge + spell.SpellData.CostOverride * 0.5f;
            if (_context.Holder.Actor.GetActorValueMax(ActorValueIndices.Magicka) - maintainCost < spell.SpellData.CostOverride)
            {
                MenuManager.ShowHUDMessage($"Cannot maintain rank {_context.ChargeLevel} {spell.Name} with current Magicka reserves", null, false);
                return;
            }

            actor.DispelSpell(_maintainSpell);
            actor.RemoveSpell(_maintainSpell);

            var activeMaintainedSpell = LoadSpellFromAV(ActorValueIndices.WaitingForPlayer);
            if (activeMaintainedSpell != null)
            {
                actor.DispelSpell(activeMaintainedSpell);
                StoreSpellInAV(null, ActorValueIndices.WaitingForPlayer);

                if (activeMaintainedSpell.FormId == spell.FormId)
                {
                    MenuManager.ShowHUDMessage($"Maintain : Dispelled", null, false);
                    return;
                }
            }

            var largeDur = 60 * 60 * 24 * 356;
            foreach (var eff in spell.Effects)
                SpellHelper.GetModifiedPower(eff).Duration = largeDur;

            Util.SimpleDeferredExecutor.Defer(() =>
            {
                DebugHelper.Print($"[Maintain] Spell CostOverride: {spell.SpellData.CostOverride}");
                _maintainSpell.Effects[0].Magnitude = maintainCost;
                _maintainSpell.Effects[0].Duration = largeDur;
                actor.AddSpell(_maintainSpell, false);
                Util.Visuals.AttachArtObject(0x56AC8, actor, 0.5f); // ShieldSpellFX
                MenuManager.ShowHUDMessage($"Maintain : {_context.Spell.Name}", null, false);
                StoreSpellInAV(spell, ActorValueIndices.WaitingForPlayer);
            }, 0xCEC, 0.1337f);
        }

        private static unsafe SpellItem LoadSpellFromAV(ActorValueIndices actorValue)
        {
            var av = PlayerCharacter.Instance.GetBaseActorValue(actorValue);
            var asUint = BitConverter.ToUInt32(BitConverter.GetBytes(av), 0);
            DebugHelper.Print($"[Maintain] Retrieve AV {actorValue} : {av} = {asUint}");
            return TESForm.LookupFormById(asUint) as SpellItem;
        }

        private static unsafe void StoreSpellInAV(SpellItem spell, ActorValueIndices actorValue)
        {
            var fid = spell?.FormId ?? 0u;
            var asFloat = BitConverter.ToSingle(BitConverter.GetBytes(fid), 0);
            DebugHelper.Print($"[Maintain] Put AV {actorValue} : {fid} = {asFloat}");
            PlayerCharacter.Instance.SetActorValue(actorValue, asFloat);
        }
    }
}
