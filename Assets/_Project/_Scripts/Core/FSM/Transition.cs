using ThePromisedRune.Core.FSM.Interfaces;

namespace ThePromisedRun.Core.FSM {
    public class Transition : ITransition {
        public IState To { get; }
        public IPredicate Condition { get; }

        public Transition(IState targetState, IPredicate condition) {
            To = targetState;
            Condition = condition;
        }
    }
}