using SpellChargingPlugin.Core;
using SpellChargingPlugin.States;
using System;
using System.Collections.Generic;

namespace SpellChargingPlugin.StateMachine
{
    public class StateFactory<TContext> where TContext : IStateHolder<TContext>
    {
        private static Dictionary<Func<State<TContext>>, State<TContext>> _states = new Dictionary<Func<State<TContext>>, State<TContext>>();
        internal State<TContext> GetOrCreate(Func<State<TContext>> creator)
        {
            if (!_states.ContainsKey(creator))
                _states.Add(creator, creator());
            return _states[creator];
        }
    }
}