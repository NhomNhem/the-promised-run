using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay.Enemy.AI;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Enemy Controller - coordinates enemy entity and AI
    /// SOLID: Single Responsibility - Only coordinates components
    /// SOLID: Dependency Inversion - Depends on interfaces
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyController : MonoBehaviour {
        [Header("Components")]
        [SerializeField] private Enemy enemy;
        [SerializeField] private EnemyAIController aiController;
        [SerializeField] private EnemyAttackHitbox attackHitbox;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Public Properties
        public Enemy Enemy => enemy;
        public EnemyAIController AIController => aiController;
        
        private void Awake() {
            // Get components if not assigned
            if (enemy == null) enemy = GetComponent<Enemy>();
            if (aiController == null) aiController = GetComponent<EnemyAIController>();
            // Try to find the hitbox on children if not assigned in inspector
            if (attackHitbox == null) attackHitbox = GetComponentInChildren<EnemyAttackHitbox>();
            
            // Setup event handlers
            SetupEventHandlers();
        }
        
        private void Start() {
            // Initialize AI controller with enemy entity
            if (aiController != null && enemy != null) {
                // Initialize AI controller only if it hasn't been initialized already
                if (!aiController.IsInitialized) {
                    aiController.Initialize(enemy);
                    if (showDebugInfo) {
                        Debug.Log($"[EnemyController] Initialized {enemy.name} with AI controller");
                    }
                } else if (showDebugInfo) {
                    Debug.Log($"[EnemyController] AI controller already initialized for {enemy.name}");
                }
            }
        }
        
        private void SetupEventHandlers() {
            if (aiController == null) return;
            
            // AI State changes
            aiController.OnStateChanged = (newState) => {
                if (showDebugInfo) {
                    Debug.Log($"[EnemyController] State changed to: {newState}");
                }
            };
            
            // Target events
            aiController.OnTargetAcquired = (target) => {
                if (showDebugInfo) {
                    Debug.Log($"[EnemyController] Target acquired: {target.GetType().Name}");
                }
            };
            
            aiController.OnTargetLost = () => {
                if (showDebugInfo) {
                    Debug.Log("[EnemyController] Target lost");
                }
            };
            
            // Enemy events
            if (enemy != null) {
                enemy.OnAttackStarted.AddListener(() => {
                    if (showDebugInfo) {
                        Debug.Log("[EnemyController] Attack started");
                    }
                });
                // Also ensure the physical hitbox is activated when the Enemy raises its attack event
                enemy.OnAttackStarted.AddListener(() => {
                    if (attackHitbox != null) attackHitbox.Activate();
                });
                
                enemy.OnAttackCompleted.AddListener(() => {
                    if (showDebugInfo) {
                        Debug.Log("[EnemyController] Attack completed");
                    }
                });
                // Deactivate physical hitbox when attack completes
                enemy.OnAttackCompleted.AddListener(() => {
                    if (attackHitbox != null) attackHitbox.Deactivate();
                });
                
                enemy.OnTargetAcquired.AddListener((target) => {
                    if (showDebugInfo) {
                        Debug.Log($"[EnemyController] Enemy acquired target: {target.GetType().Name}");
                    }
                    // Notify AIController when Enemy acquires target directly (detectors or other systems)
                    if (aiController != null) {
                        aiController.SetTarget(target);
                    }
                });

                enemy.OnTargetLost.AddListener((target) => {
                    if (showDebugInfo) Debug.Log("[EnemyController] Enemy lost target (from Enemy event)");
                    if (aiController != null) aiController.ClearTarget();
                });
            }
        }
        
        // Handle damage events and pass to AI controller
        public void OnDamaged(DamageInfo damageInfo) {
            if (aiController != null) {
                aiController.OnDamaged(damageInfo);
            }
        }
        
        // Animation event handlers
        public void OnHitboxActivate() {
            // Prefer activating the physical hitbox if present (so animation events toggle the collider)
            if (attackHitbox != null) {
                attackHitbox.Activate();
                if (showDebugInfo) Debug.Log("[EnemyController] Attack hitbox activated via animation event");
            } else if (enemy != null) {
                // Fallback: invoke enemy events so other systems can react
                enemy.OnAttackStarted?.Invoke();
                if (showDebugInfo) Debug.LogWarning("[EnemyController] No EnemyAttackHitbox found; invoked Enemy.OnAttackStarted instead");
            }
        }
        
        public void OnHitboxDeactivate() {
            if (attackHitbox != null) {
                attackHitbox.Deactivate();
                if (showDebugInfo) Debug.Log("[EnemyController] Attack hitbox deactivated via animation event");
            } else if (enemy != null) {
                enemy.OnAttackCompleted?.Invoke();
                if (showDebugInfo) Debug.LogWarning("[EnemyController] No EnemyAttackHitbox found; invoked Enemy.OnAttackCompleted instead");
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
        
        // Unity Events
        private void OnEnable() {
            // Re-enable AI when component is enabled
            if (aiController != null) {
                aiController.IsActive = true;
            }
        }
        
        private void OnDisable() {
            // Disable AI when component is disabled
            if (aiController != null) {
                aiController.IsActive = false;
            }
        }
    }
}
