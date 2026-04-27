using UnityEngine;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Attack state - enemy attacks target
    /// SOLID: Single Responsibility - Only handles attack behavior
    /// </summary>
    public class AttackState : EnemyStateBase {
        private float _attackTimer;
        private readonly float _attackCooldown = 1f;
        
        public AttackState(IEnemyEntity enemyEntity, IEnemyAI aiController) 
            : base(enemyEntity, aiController, EnemyAIState.Attack) {
        }
        
        protected override void OnEnter() {
            _attackTimer = 0f;
            Debug.Log($"[{EnemyEntity}] Entering attack state");
        }
        
        protected override void OnExit() {
            // Cleanup when leaving attack state
        }
        
        protected override void OnUpdate() {
            // Check if still has target and can attack
            if (!HasTarget() || !CanAttack()) {
                AIController.ChangeState(EnemyAIState.Chase);
                return;
            }
            
            // Face target
            EnemyEntity.FaceTarget(EnemyEntity.CurrentTarget);
            
            // Attack if ready
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f) {
                PerformAttack();
                _attackTimer = _attackCooldown;
            }
            
            // Transition back to chase after attack animation
            // This would be triggered by animation events, but for now we'll use a timer
            if (_attackTimer <= _attackCooldown * 0.5f) {
                AIController.ChangeState(EnemyAIState.Chase);
            }
        }
        
        private void PerformAttack() {
            EnemyEntity.Attack();
            Debug.Log($"[{EnemyEntity}] Attacking target");
        }
    }
}
