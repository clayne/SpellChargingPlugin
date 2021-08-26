using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.StateMachine
{
    public abstract class State<TContext> where TContext : IStateHolder<TContext>
    {
        protected StateFactory<TContext> _factory;
        protected TContext _context;

        public State(StateFactory<TContext> factory, TContext context)
        {
            _factory = factory;
            _context = context;
        }

        protected void TransitionTo<T>(Func<T> creator) where T : State<TContext>
        {
            State<TContext> newState = _factory.GetOrCreate(creator);

            DebugHelper.Print($"State change: {_context.CurrentState.GetType().Name} => {newState.GetType().Name}");

            _context.CurrentState = newState;
        }

        public abstract void Update(float diff);
    }
}
