using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Enemy Attack state — plays Attack animation, activates hitbox via timer.
    /// Uses timer-based hitbox since animation clips have no events yet.
    /// </summary>
    public class EnemyAttackFSMState : IState {
        private readonly Enemy              _enemy;
        private readonly EnemyAttackHitbox  _hitbox;

        private const float AttackDuration  = 0.6f;  // total attack state duration
        private const float HitboxStart     = 0.2f;  // when hitbox activates
        private const float HitboxEnd       = 0.45f; // when hitbox deactivates

        private float _timer;

        public bool AttackComplete => _timer >= AttackDuration;

        public EnemyAttackFSMState(Enemy enemy, EnemyAttackHitbox hitbox) {
            _enemy  = enemy;
            _hitbox = hitbox;
        }

        public void OnEnter() {
            _timer = 0f;
            _enemy.StopMovement();
            _enemy.FaceTarget(_enemy.CurrentTarget);
            // Direct CrossFade — bypasses transition graph
            _enemy.Animator?.SetBool("IsMoving", false);
            _enemy.Animator?.CrossFade("Attack", 0.05f, 0);
            _enemy.OnAttackStarted?.Invoke();
        }

        public void OnUpdate() {
            _timer += Time.deltaTime;

            // Timer-based hitbox activation
            if (_hitbox != null) {
                if (_timer >= HitboxStart && _timer < HitboxEnd)
                    _hitbox.Activate();
                else if (_timer >= HitboxEnd)
                    _hitbox.Deactivate();
            }
        }

        public void OnFixedUpdate() { }

        public void OnExit() {
            _hitbox?.Deactivate();
            _enemy.OnAttackCompleted?.Invoke();
            // Reset attack cooldown on Enemy
            _enemy.ResetAttackCooldown();
        }
    }
}
