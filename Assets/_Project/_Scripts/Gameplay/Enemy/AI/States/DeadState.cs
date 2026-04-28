using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Dead state - enemy is dead and inactive
    /// SOLID: Single Responsibility - Only handles death behavior
    /// </summary>
    public class DeadState : EnemyStateBase {
        public DeadState(IEnemyEntity enemyEntity, IEnemyAI aiController) 
            : base(enemyEntity, aiController, EnemyAIState.Dead) {
        }
        
        protected override void OnEnter() {
            EnemyEntity.StopMovement();
            Debug.Log($"[{EnemyEntity}] Entering dead state");
            
            // Play death animation if available
            var animator = EnemyEntity.GameObject.GetComponentInChildren<Animator>();
            if (animator != null) {
                animator.SetTrigger("Death");
            }
            
            // Disable physics
            var rigidbody = EnemyEntity.GameObject.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.isKinematic = true;
            }
            
            // Clear target
            EnemyEntity.ClearTarget();
        }
        
        protected override void OnExit() {
            // Cannot exit dead state
        }
        
        protected override void OnUpdate() {
            // Dead state has no update behavior
        }
        
        public override void OnDamaged(DamageInfo damageInfo) {
            // Cannot damage dead enemies
        }
    }
}
