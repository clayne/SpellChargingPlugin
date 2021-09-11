using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using SpellChargingPlugin.Core;

namespace SpellChargingPlugin.StateMachine.States
{
    public class Overcharging : OverchargingBase
    {
        private bool _isDualCharge;
        public Overcharging(ChargingSpell context) : base(context)
        {
        }

        protected override void OnUpdate(float elapsedSeconds)
        {
            var handState = SpellHelper.GetSpellAndState(_context.Holder.Actor, _context.Slot);

            // for some reason IsDualCasting does not always return true even if you are dual casting??? just keep checking
            _isDualCharge = _isDualCharge || _context.Holder.IsDualCasting();

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
                    TransitionTo(() => new Released(_context, _isDualCharge));
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