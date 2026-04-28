using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay.Enemy.AI;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Example implementation showing how to use the new Enemy architecture
    /// This replaces EnemyController.cs and EnemyControllerRefactored.cs
    /// </summary>
    public class EnemyExample : MonoBehaviour {
        [Header("Components")]
        [SerializeField] private Enemy enemy;
        [SerializeField] private EnemyAIController aiController;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private void Awake() {
            // Get components if not assigned
            if (enemy == null) enemy = GetComponent<Enemy>();
            if (aiController == null) aiController = GetComponent<EnemyAIController>();
            
            // Setup event handlers
            SetupEventHandlers();
        }
        
        private void Start() {
            // Initialize AI controller with enemy entity
            if (aiController != null && enemy != null) {
                aiController.Initialize(enemy);
                
                if (showDebugInfo) {
                    Debug.Log($"[EnemyExample] Initialized {enemy.name} with AI controller");
                }
            }
        }
        
        private void SetupEventHandlers() {
            if (aiController == null) return;
            
            // AI State changes
            aiController.OnStateChanged += (newState) => {
                if (showDebugInfo) {
                    Debug.Log($"[EnemyExample] State changed to: {newState}");
                }
            };
            
            // Target events
            aiController.OnTargetAcquired += (target) => {
                if (showDebugInfo) {
                    Debug.Log($"[EnemyExample] Target acquired: {target.GetType().Name}");
                }
            };
            
            aiController.OnTargetLost += () => {
                if (showDebugInfo) {
                    Debug.Log("[EnemyExample] Target lost");
                }
            };
            
            // Enemy events
            if (enemy != null) {
                enemy.OnAttackStarted.AddListener(() => {
                    if (showDebugInfo) {
                        Debug.Log("[EnemyExample] Attack started");
                    }
                });
                
                enemy.OnAttackCompleted.AddListener(() => {
                    if (showDebugInfo) {
                        Debug.Log("[EnemyExample] Attack completed");
                    }
                });
                
                enemy.OnTargetAcquired.AddListener((target) => {
                    if (showDebugInfo) {
                        Debug.Log($"[EnemyExample] Enemy acquired target: {target.GetType().Name}");
                    }
                });
            }
        }
        
        // Handle damage events and pass to AI controller
        public void OnDamaged(DamageInfo damageInfo) {
            if (aiController != null) {
                aiController.OnDamaged(damageInfo);
            }
        }
        
        // Public methods for external control
        public void ForceState(EnemyAIState state) {
            if (aiController != null) {
                aiController.ForceState(state);
            }
        }
        
        public string GetCurrentState() {
            return aiController?.GetCurrentStateName() ?? "Unknown";
        }
        
        public bool IsAttacking() {
            return enemy?.IsAttacking ?? false;
        }
        
        public bool HasTarget() {
            return enemy?.HasTarget ?? false;
        }
        
        // Debug visualization
        private void OnDrawGizmosSelected() {
            if (enemy == null) return;
            
            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(enemy.transform.position, enemy.DetectionRadius);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(enemy.transform.position, enemy.AttackRange);
            
            // Draw line to target
            if (enemy.HasTarget && enemy.CurrentTarget != null) {
                Gizmos.color = Color.green;
                Vector3 targetPos = ((MonoBehaviour)enemy.CurrentTarget).transform.position;
                Gizmos.DrawLine(enemy.transform.position, targetPos);
            }
        }
    }
}
