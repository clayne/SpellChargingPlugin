using NetScriptFramework.SkyrimSE;
using System;
using System.Linq;

namespace SpellChargingPlugin.Core
{
    internal class MaintainedSpell
    {
        private const int largeDur = 60 * 60 * 24 * 356 * 7;

        private static ActorValueIndices _misusedActorValue = ActorValueIndices.WaitingForPlayer;
        private static SpellItem _maintainDebuffSpell;
        private ChargingActor _actor;
        private SpellItem _spell;
        private bool _active = false;
        private Util.SimpleTimer _updateTimer = new Util.SimpleTimer();

        public bool Dispelled { get; private set; } = false;


        public MaintainedSpell(ChargingActor chargingActor, SpellItem spell)
        {
            this._actor = chargingActor;
            this._spell = spell;

            if (_maintainDebuffSpell != null)
                return;
            _maintainDebuffSpell = SpellCharging.FormList.All[0] as SpellItem;
        }

        public void Dispel(bool onlyDispelDebuff = false)
        {
            _actor.Actor.RemoveSpell(_maintainDebuffSpell);

            var activeMaintainedSpell = SpellHelper.LoadSpellFromAV(_misusedActorValue);
            if (activeMaintainedSpell != null)
            {
                if (!onlyDispelDebuff)
                    _actor.Actor.DispelSpell(activeMaintainedSpell);
                SpellHelper.StoreSpellInAV(null, _misusedActorValue);

                Util.Visuals.AttachArtObject(0x56AC8, _actor.Actor, 0.25f); // ShieldSpellFX
                MenuManager.ShowHUDMessage($"Maintain : {_spell.Name} Dispelled", null, false);
                Dispelled = true;
            }
        }

        public void Apply(int chargeLevel)
        {
            var maintainCost = chargeLevel * Settings.Instance.MagickaPerCharge + _spell.SpellData.CostOverride * 0.5f;

            foreach (var eff in _spell.Effects)
                SpellHelper.GetModifiedPower(eff).Duration = largeDur;

            Util.Visuals.AttachArtObject(0x56AC8, _actor.Actor, 0.5f); // ShieldSpellFX
            SpellHelper.StoreSpellInAV(_spell, _misusedActorValue);

            // can't go TOO fast or the game shits itself and ignores this
            Util.SimpleDeferredExecutor.Defer(() =>
            {
                _maintainDebuffSpell.Effects[0].Magnitude = maintainCost;
                _actor.Actor.AddSpell(_maintainDebuffSpell, false);
                _active = true;
                MenuManager.ShowHUDMessage($"Maintain : {_spell.Name}", null, false);
            }, 0x1234, 0.5f);
        }

        public static MaintainedSpell TryRestore(ChargingActor chargingActor)
        {
            var storedSpell = SpellHelper.LoadSpellFromAV(_misusedActorValue);
            if (storedSpell != null)
                return new MaintainedSpell(chargingActor, storedSpell);
            return null;
        }

        public void Update(float elapsedSeconds)
        {
            if (!_active || Dispelled)
                return;

            _updateTimer.Update(elapsedSeconds);
            if (!_updateTimer.HasElapsed(1f, out _))
                return;

            bool onlyDispelDebuff = false;
            if (_actor.Actor.HasMagicEffect(_spell.AVEffectSetting))
            {
                var av = _actor.Actor.ActiveEffects.FirstOrDefault(e => e.BaseEffect.FormId == _spell.AVEffectSetting.FormId);
                if (av == null)
                    return;
                if (av.Duration > largeDur / 2)
                    return;
                onlyDispelDebuff = true;
            }
            Dispel(onlyDispelDebuff);
        }
    }
}