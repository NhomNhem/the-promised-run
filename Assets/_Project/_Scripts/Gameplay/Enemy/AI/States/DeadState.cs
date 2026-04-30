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

            // Disable NavMeshAgent and collider immediately
            var nav = EnemyEntity.GameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.enabled = false;

            var col = EnemyEntity.GameObject.GetComponent<UnityEngine.CapsuleCollider>();
            if (col != null) col.enabled = false;

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

            // Destroy enemy after death animation (2.5s)
            var mono = EnemyEntity.GameObject.GetComponent<UnityEngine.MonoBehaviour>();
            mono?.StartCoroutine(DestroyAfterDelay(2.5f));
        }

        private System.Collections.IEnumerator DestroyAfterDelay(float delay) {
            yield return new UnityEngine.WaitForSeconds(delay);
            if (EnemyEntity?.GameObject != null)
                UnityEngine.Object.Destroy(EnemyEntity.GameObject);
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
