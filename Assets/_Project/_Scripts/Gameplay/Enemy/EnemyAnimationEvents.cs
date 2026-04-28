using UnityEngine;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Handles AnimationEvents on the enemy visual GameObject and forwards them to EnemyController
    /// </summary>
    public class EnemyAnimationEvents : MonoBehaviour {
        private EnemyController _enemyController;

        private void Awake() {
            // Find the EnemyController on the parent/root GameObject
            _enemyController = GetComponentInParent<EnemyController>();
            if (_enemyController == null) {
                // Try to find Enemy component as fallback
                var enemy = GetComponentInParent<Enemy>();
                if (enemy != null) {
                    _enemyController = enemy.GetComponent<EnemyController>();
                }
                
                if (_enemyController == null) {
                    Debug.LogError("EnemyAnimationEvents: Could not find EnemyController in parent hierarchy!");
                }
            }
        }

        /// <summary>
        /// Called by animation event to activate attack hitbox
        /// </summary>
        public void OnHitboxActivate() {
            _enemyController?.OnHitboxActivate();
        }

        /// <summary>
        /// Called by animation event to deactivate attack hitbox
        /// </summary>
        public void OnHitboxDeactivate() {
            _enemyController?.OnHitboxDeactivate();
        }
    }
}
