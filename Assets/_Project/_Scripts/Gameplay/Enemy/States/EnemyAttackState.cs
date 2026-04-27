using UnityEngine;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy {
    public class EnemyAttackState : EnemyBaseState {
        private const float AttackCooldown = 1.2f;
        private float _timer;

        public bool IsComplete => _timer >= AttackCooldown;

        public EnemyAttackState(EnemyController enemy, Animator animator)
            : base(enemy, animator) { }

        public override void OnEnter() {
            _timer = 0f;
            _enemy.Rb.linearVelocity = Vector3.zero;
            _animator.SetBool("IsMoving", false);
            _animator.SetTrigger("Attack");
            _enemy.FaceTarget();
            TryDealDamage();
        }

        public override void OnUpdate() {
            _timer += Time.deltaTime;
        }

        private void TryDealDamage() {
            if (_enemy.Target == null) return;
            var damageable = _enemy.Target.GetComponent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;

            var info = new DamageInfo(
                _enemy.AttackDamage,
                _enemy.Target.position,
                (_enemy.Target.position - _enemy.transform.position).normalized,
                _enemy.gameObject
            );
            damageable.TakeDamage(_enemy.AttackDamage, info);
        }
    }
}
