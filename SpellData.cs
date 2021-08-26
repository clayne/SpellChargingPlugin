using NetScriptFramework.SkyrimSE;
using System;
using System.Linq;

namespace SpellChargingPlugin
{
    public class SpellData
    {
        public enum ChargingState { Idle, Charging, Cancelled, Released }
        
        public SpellData(SpellItem spell)
        {
            Spell = spell;
            SpellHelper.CacheSpellBasePower(spell);
            BasePower = SpellHelper.GetSpellBasePower(spell);
        }

        public SpellItem Spell { get; set; }
        public SpellPower[] BasePower { get; set; }
        public ChargingState State { get; set; } = ChargingState.Idle;

        private float _chargeDuration = 0.0f;
        internal void OnUpdate(float diff)
        {
            switch (State)
            {
                case ChargingState.Charging:
                    Charge(diff);
                    return;
                case ChargingState.Cancelled:
                    Cancel();
                    return;
                case ChargingState.Released:
                    return;
            }
        }

        private void Cancel()
        {
            ResetSpellPower();
        }

        public void ResetSpellPower()
        {
            for (int i = 0; i < Spell.Effects.Count; i++)
            {
                var eff = Spell.Effects[i];
                if (eff == null)
                    continue;

                var baseMag = BasePower[i].Magnitude;
                var baseDur = BasePower[i].Duration;
                DebugHelper.Print($"Resetting {Spell.Name}.{eff.Effect.Name} to [Mag: {baseMag}, Dur: {baseDur}]");

                eff.Magnitude = BasePower[i].Magnitude;
                eff.Duration = BasePower[i].Duration;
            }
        }

        private void Charge(float diff)
        {
            _chargeDuration += diff;
            if (_chargeDuration >= Settings.ChargeInterval)
            {
                Empower(Settings.ChargeIncrement);
                if (Settings.ApplyVisuals)
                    ApplyVisual();
                _chargeDuration = 0.0f;
            }
        }

        private void ApplyVisual()
        {
            throw new NotImplementedException();
        }

        private void Empower(float powerGain)
        {
            for (int i = 0; i < Spell.Effects.Count; i++)
            {
                var eff = Spell.Effects[i];
                if (eff == null)
                    continue;

                // boost magnitude and/or duration, but only apply half boost if spell has both (balance)
                bool hasMag = BasePower[i].Magnitude > 0;
                bool hasDur = BasePower[i].Duration > 0;
                powerGain *= hasDur && hasMag ? 0.5f : 1f;

                if (hasMag)
                {
                    var curMag = eff.Magnitude;
                    var magGain = BasePower[i].Magnitude * powerGain;
                    var newMag = curMag + magGain;

                    DebugHelper.Print($"Empowering {Spell.Name}.{eff.Effect.Name}: Mag [{curMag} > {newMag}]");
                    eff.Magnitude = newMag;
                }
                if (hasDur)
                {
                    var curDur = eff.Duration;
                    var durGain = BasePower[i].Duration * powerGain;
                    var newDur = (int)(curDur + durGain);

                    DebugHelper.Print($"Empowering {Spell.Name}.{eff.Effect.Name}: Mag [{curDur} > {newDur}]");
                    eff.Duration = newDur;
                }
            }
        }
    }
}