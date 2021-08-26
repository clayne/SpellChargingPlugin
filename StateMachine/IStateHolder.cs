namespace SpellChargingPlugin.StateMachine
{
    public interface IStateHolder<TContext> where TContext : IStateHolder<TContext>
    {
        State<TContext> CurrentState { get; set; }
    }
}