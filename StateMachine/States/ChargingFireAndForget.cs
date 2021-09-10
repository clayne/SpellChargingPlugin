using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;

namespace SpellChargingPlugin.StateMachine.States
{
    public class ChargingFireAndForget : ChargingBase
    {
        public ChargingFireAndForget(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetSpellAndState(_context.Holder.Actor, _context.Slot);

            switch (handState?.State)
            {
                case MagicCastingStates.Charged:
                    float _accelerationFactor = 1f;
                    if (Settings.Instance.EnableAcceleration)
                        _accelerationFactor = (Settings.Instance.AccelerationHalfTime + this._timeInState) / Settings.Instance.AccelerationHalfTime;
                    _chargingTimer.Update(elapsedSeconds * _accelerationFactor);
                    if (!_chargingTimer.HasElapsed(_inverseChargesPerSecond, out _))
                        return;
                    if (!_context.Holder.TryDrainMagicka(Settings.Instance.MagickaPerCharge))
                        return;
                    _context.AddCharge();
                    break;
                case MagicCastingStates.Released:
                    TransitionTo(() => new Released(_context));
                    break;
                case MagicCastingStates.None:
                case null:
                default:
                    TransitionTo(() => new Canceled(_context));
                    break;
            }
        }
    }
}