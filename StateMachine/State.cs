using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine
{
    public abstract class State<TContext> where TContext : IStateHolder<TContext>
    {
        protected TContext _context;
        protected float _timeInState = 0f;

        public State(TContext context)
        {
            _context = context;
        }

        protected void TransitionTo<T>(Func<T> creator) where T : State<TContext>
        {
            OnExitState();
            State<TContext> newState = creator();
            newState._timeInState = 0f;
            newState.OnEnterState();

            DebugHelper.Print($"[State] Change: {_context.CurrentState.GetType().Name} => {newState.GetType().Name}");

            _context.CurrentState = newState;
        }

        public virtual void Update(float elapsedSeconds)
        {
            _timeInState += elapsedSeconds;
            OnUpdate(elapsedSeconds);
        }

        protected abstract void OnUpdate(float elapsedSeconds);
        protected virtual void OnEnterState() { }
        protected virtual void OnExitState() { }
    }
}
