using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy {
    public abstract class EnemyBaseState : IState {
        protected readonly EnemyController _enemy;
        protected readonly Animator        _animator;

        protected EnemyBaseState(EnemyController enemy, Animator animator) {
            _enemy    = enemy;
            _animator = animator;
        }

        public virtual void OnEnter()       { }
        public virtual void OnUpdate()      { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnExit()        { }
    }
}
