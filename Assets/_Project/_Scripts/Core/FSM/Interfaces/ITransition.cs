namespace ThePromisedRun.Core.FSM.Interfaces {
    public interface ITransition {
        IState To { get; }
        IPredicate Condition { get; }
    }
}