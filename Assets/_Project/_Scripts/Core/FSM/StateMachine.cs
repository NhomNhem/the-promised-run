using System;
using System.Collections.Generic;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Core.FSM {
    public class StateMachine {
        private StateNode _currentState;
        private readonly Dictionary<Type, StateNode> _stateNodes = new();
        private readonly HashSet<ITransition> _anyTransitions = new();

        public void Update() {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.To);

            _currentState?.State?.OnUpdate(); 
        }

        public void FixedUpdate() {
            _currentState?.State?.OnFixedUpdate();
        }

        public void SetState(IState state) {
            _currentState = GetOrAddNode(state);
            _currentState?.State?.OnEnter();
        }

        private void ChangeState(IState state) {
            if (state == _currentState?.State) return;

            _currentState?.State?.OnExit();
            _currentState = GetOrAddNode(state);
            _currentState?.State?.OnEnter();
        }

        private ITransition GetTransition() {
            foreach (var transition in _anyTransitions)
                if (transition.Condition.Evaluate())
                    return transition;

            if (_currentState != null) {
                foreach (var transition in _currentState.Transitions)
                    if (transition.Condition.Evaluate())
                        return transition;
            }

            return null;
        }

        public void AddTransition(IState from, IState to, IPredicate condition) =>
            GetOrAddNode(from).AddTransition(to, condition);

        public void AddAnyTransition(IState to, IPredicate condition) =>
            _anyTransitions.Add(new Transition(to, condition));

        private StateNode GetOrAddNode(IState state) {
            var type = state.GetType();
            if (!_stateNodes.TryGetValue(type, out var node)) {
                node = new StateNode(state);
                _stateNodes.Add(type, node);
            }

            return node;
        }
    }
}