using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Enemy Idle state — stands still, plays Idle animation.
    /// Transitions handled by EnemyBrain predicates.
    /// </summary>
    public class EnemyIdleState : IState {
        private readonly Enemy _enemy;

        public EnemyIdleState(Enemy enemy) {
            _enemy = enemy;
        }

        public void OnEnter() {
            _enemy.StopMovement();
            _enemy.Animator?.SetBool("IsMoving", false);
        }

        public void OnUpdate() { }
        public void OnFixedUpdate() { }

        public void OnExit() { }
    }
}
