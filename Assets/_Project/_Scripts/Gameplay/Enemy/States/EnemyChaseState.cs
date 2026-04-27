using UnityEngine;

namespace ThePromisedRun.Gameplay.Enemy {
    public class EnemyChaseState : EnemyBaseState {
        public EnemyChaseState(EnemyController enemy, Animator animator)
            : base(enemy, animator) { }

        public override void OnEnter() {
            _animator.SetBool("IsMoving", true);
        }

        public override void OnFixedUpdate() {
            if (_enemy.Target == null) return;

            Vector3 dir = (_enemy.Target.position - _enemy.transform.position).normalized;
            dir.y = 0f;
            _enemy.Rb.linearVelocity = new Vector3(
                dir.x * _enemy.MoveSpeed,
                _enemy.Rb.linearVelocity.y,
                dir.z * _enemy.MoveSpeed);

            _enemy.FaceTarget();
        }

        public override void OnExit() {
            _enemy.Rb.linearVelocity = Vector3.zero;
        }
    }
}
