namespace ThePromisedRune.Core.FSM.Interfaces {
    public interface IState {
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
    }
}