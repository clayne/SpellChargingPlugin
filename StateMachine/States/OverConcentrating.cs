﻿using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;

namespace SpellChargingPlugin.StateMachine.States
{
    internal class OverConcentrating : OverchargingBase
    {
        public static int InstanceCount { get; private set; } = 0;

        public OverConcentrating(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetSpellAndState(_context.Owner.Character, _context.Slot);

            switch (handState?.State)
            {
                case MagicCastingStates.Concentrating:
                    float _accelerationFactor = 1f;
                    if (Settings.Instance.EnableAcceleration)
                        _accelerationFactor = (Settings.Instance.AccelerationHalfTime + this._timeInState) / Settings.Instance.AccelerationHalfTime;
                    _chargingTimer.Update(elapsedSeconds * _accelerationFactor);
                    if (!_chargingTimer.HasElapsed(_inverseChargesPerSecond, out _))
                        return;
                    if (!_context.Owner.TryConsumeMagicka(Settings.Instance.MagickaPerCharge))
                        return;
                    _context.AddCharge();
                    SpellPowerManager.Instance.ApplyModifiers(_context);
                    break;
                case MagicCastingStates.None:
                case null:
                default:
                    TransitionTo(() => new Canceled(_context));
                    break;
            }
        }

        // These SHOULD be enough to track whether or not the player is still firing spells and keep the "victims" inside
        // ActiveEffectTracker alive until the player stops, at which point there SHOULD be no more ActiveEffects to update
        // and the cache can be thrown away
        protected override void OnEnterState()
        {
            ++InstanceCount;
        }
        protected override void OnExitState()
        {
            if (--InstanceCount == 0)
                ActiveEffectTracker.Instance.Clear();
        }
    }
}