using UnityEngine;

namespace ThePromisedRun.Gameplay.Enemy {
    public class EnemyIdleState : EnemyBaseState {
        public EnemyIdleState(EnemyController enemy, Animator animator)
            : base(enemy, animator) { }

        public override void OnEnter() {
            _animator.SetBool("IsMoving", false);
            _enemy.Rb.linearVelocity = Vector3.zero;
        }
    }
}
