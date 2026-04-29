using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Enemy.AI.ScriptableObjects;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy.AI {
    /// <summary>
    /// Enhanced Enemy AI Controller using NavMesh and Scriptable Objects
    /// Designed for use with Odin Inspector for visual AI configuration
    /// </summary>
    public class EnemyAIControllerNavMesh : MonoBehaviour, IEnemyAI {
        [Header("AI Configuration")]
        [SerializeField] private EnemyAISettings aiSettings;
        [SerializeField] private EnemyAIBehavior aiBehavior;
        
        [Header("Components")]
        [SerializeField] private Enemy enemy;
        [SerializeField] private EnemyAINavMeshController navMeshController;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool showDecisionLogs = false;
        
        // State management
        private EnemyAIState _currentState = EnemyAIState.Idle;
        private float _stateTimer;
        private Vector3 _patrolCenter;
        private float _patrolAngle;
        
        // Events
        public System.Action<EnemyAIState> OnStateChanged { get; set; }
        public System.Action<IDamageable> OnTargetAcquired { get; set; }
        public System.Action OnTargetLost { get; set; }
        
        // Properties
        public EnemyAIState CurrentState => _currentState;
        public bool IsActive { get; set; } = true;
        public IEnemyEntity Enemy => enemy;
        
        private void Awake() {
            // Get components if not assigned
            if (enemy == null) enemy = GetComponent<Enemy>();
            if (navMeshController == null) navMeshController = GetComponent<EnemyAINavMeshController>();
            
            // Initialize events
            OnStateChanged = (state) => { };
            OnTargetAcquired = (target) => { };
            OnTargetLost = () => { };
        }
        
        private void Start() {
            // Validate settings
            if (aiSettings == null) {
                Debug.LogError("EnemyAIControllerNavMesh: AI Settings not assigned!");
                return;
            }
            
            if (aiBehavior == null) {
                Debug.LogWarning("EnemyAIControllerNavMesh: AI Behavior not assigned, using default behavior");
            }
            
            // Initialize patrol center
            _patrolCenter = transform.position;
            
            // Start in idle state
            ChangeState(EnemyAIState.Idle);
        }
        
        private void Update() {
            if (!IsActive || enemy == null || !enemy.IsAlive) return;
            
            UpdateState();
            ProcessAI();
        }
        
        private void UpdateState() {
            _stateTimer -= Time.deltaTime;
            
            // Check for state transitions based on behavior
            if (aiBehavior != null) {
                var newState = aiBehavior.GetHighestPriorityState(enemy, this);
                if (newState != _currentState) {
                    ChangeState(newState);
                }
            }
        }
        
        private void ProcessAI() {
            switch (_currentState) {
                case EnemyAIState.Idle:
                    ProcessIdleState();
                    break;
                    
                case EnemyAIState.Patrol:
                    ProcessPatrolState();
                    break;
                    
                case EnemyAIState.Chase:
                    ProcessChaseState();
                    break;
                    
                case EnemyAIState.Attack:
                    ProcessAttackState();
                    break;
                    
                case EnemyAIState.Stunned:
                    ProcessStunnedState();
                    break;
                    
                case EnemyAIState.Dead:
                    ProcessDeadState();
                    break;
            }
        }
        
        #region State Processing
        
        private void ProcessIdleState() {
            // Look for targets
            if (CheckForTargets()) {
                ChangeState(EnemyAIState.Chase);
                return;
            }
            
            // Transition to patrol after idle timeout
            if (_stateTimer <= 0f) {
                ChangeState(EnemyAIState.Patrol);
            }
        }
        
        private void ProcessPatrolState() {
            if (navMeshController == null) return;
            
            // Check for targets
            if (CheckForTargets()) {
                ChangeState(EnemyAIState.Chase);
                return;
            }
            
            // Update patrol movement
            if (_stateTimer <= 0f) {
                // Generate new patrol point
                navMeshController.StartPatrol(_patrolCenter);
                _stateTimer = aiSettings.patrolWaitTime + Random.Range(-1f, 1f);
            }
        }
        
        private void ProcessChaseState() {
            if (navMeshController == null || !enemy.HasTarget) {
                ChangeState(EnemyAIState.Idle);
                return;
            }
            
            // Move towards target
            var targetPos = enemy.LastKnownTargetPosition;
            navMeshController.MoveTowards(targetPos);
            
            // Check if should attack
            if (navMeshController.ShouldAttack()) {
                ChangeState(EnemyAIState.Attack);
                return;
            }
            
            // Check if lost target
            if (enemy.TimeSinceLastSeenTarget > aiSettings.loseTargetTime) {
                ChangeState(EnemyAIState.Patrol);
                OnTargetLost?.Invoke();
            }
        }
        
        private void ProcessAttackState() {
            if (!enemy.HasTarget || !navMeshController.IsTargetInRange(enemy.CurrentTarget)) {
                ChangeState(EnemyAIState.Chase);
                return;
            }
            
            // Stop movement for attack
            navMeshController.StopMovement();
            
            // Face target
            navMeshController.FaceTarget(enemy.CurrentTarget);
            
            // Attack if ready
            if (enemy.CanAttack) {
                enemy.Attack();
                _stateTimer = aiSettings.attackCooldown;
            }
            
            // Transition back to chase after attack
            if (_stateTimer <= 0f) {
                ChangeState(EnemyAIState.Chase);
            }
        }
        
        private void ProcessStunnedState() {
            // Stop all movement
            navMeshController?.StopMovement();
            
            // Wait for stun to end
            if (_stateTimer <= 0f) {
                if (enemy.HasTarget) {
                    ChangeState(EnemyAIState.Chase);
                } else {
                    ChangeState(EnemyAIState.Idle);
                }
            }
        }
        
        private void ProcessDeadState() {
            // Stop all movement
            navMeshController?.StopMovement();
            
            // Dead state doesn't transition
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool CheckForTargets() {
            // Simple overlap sphere detection
            var colliders = Physics.OverlapSphere(
                transform.position, 
                aiSettings.detectionRadius, 
                LayerMask.GetMask("Player")
            );
            
            foreach (var collider in colliders) {
                var damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive) {
                    // Check if target is in detection angle
                    var direction = (collider.transform.position - transform.position).normalized;
                    var angle = Vector3.Angle(transform.forward, direction);
                    
                    if (angle <= aiSettings.detectionAngle * 0.5f) {
                        enemy.SetTarget(damageable);
                        OnTargetAcquired?.Invoke(damageable);
                        
                        if (showDecisionLogs) {
                            Debug.Log($"[{gameObject.name}] Target acquired: {damageable.GetType().Name}");
                        }
                        
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        #endregion
        
        #region IEnemyAI Implementation
        
        public void Initialize(IEnemyEntity enemyEntity) {
            enemy = enemyEntity as Enemy;
            
            if (aiSettings != null) {
                // Apply behavior modifiers
                aiBehavior?.ApplyModifiers(enemy);
                
                // Configure NavMesh controller
                if (navMeshController != null) {
                    // NavMesh controller will use the AI settings
                }
            }
        }
        
        public void UpdateAI() {
            // This method is called by IEnemyAI interface, but we handle updates in Update()
        }
        
        public void ChangeState(EnemyAIState newState) {
            if (_currentState == newState) return;
            
            var oldState = _currentState;
            _currentState = newState;
            
            // Reset state timer
            switch (newState) {
                case EnemyAIState.Idle:
                    _stateTimer = 2f; // Idle for 2 seconds
                    break;
                    
                case EnemyAIState.Patrol:
                    _stateTimer = aiSettings.patrolWaitTime;
                    break;
                    
                case EnemyAIState.Attack:
                    _stateTimer = aiSettings.attackCooldown;
                    break;
                    
                case EnemyAIState.Stunned:
                    _stateTimer = 2f; // Stun for 2 seconds
                    break;
                    
                case EnemyAIState.Dead:
                    _stateTimer = float.MaxValue; // Permanent
                    break;
            }
            
            if (debugMode) {
                Debug.Log($"[{gameObject.name}] State changed: {oldState} -> {newState}");
            }
            
            OnStateChanged?.Invoke(newState);
        }
        
        public void OnDamaged(DamageInfo damageInfo) {
            if (!enemy.IsAlive) return;
            
            // Check for response rules
            if (aiBehavior != null) {
                foreach (var rule in aiBehavior.responseRules) {
                    if (rule.ShouldRespond(damageInfo, enemy)) {
                        ChangeState(rule.responseState);
                        
                        if (showDecisionLogs) {
                            Debug.Log($"[{gameObject.name}] Responding to {(damageInfo.IsOverloadBoosted ? "overload" : "normal")} damage with {rule.responseState}");
                        }
                        
                        return;
                    }
                }
            }
            
            // Default response: if stunned by overload damage
            if (damageInfo.IsOverloadBoosted) {
                ChangeState(EnemyAIState.Stunned);
            }
        }
        
        public void SetTarget(IDamageable target) {
            enemy?.SetTarget(target);
        }
        
        public void ClearTarget() {
            enemy?.ClearTarget();
        }
        
        #endregion
        
        #region Public Methods
        
        public void ForceState(EnemyAIState state) {
            ChangeState(state);
        }
        
        public string GetCurrentStateName() {
            return _currentState.ToString();
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected() {
            if (aiSettings == null || !debugMode) return;
            
            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aiSettings.detectionRadius);
            
            // Draw detection angle
            if (aiSettings.detectionAngle < 360f) {
                var halfAngle = aiSettings.detectionAngle * 0.5f;
                var leftDir = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;
                var rightDir = Quaternion.Euler(0, halfAngle, 0) * transform.forward;
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + leftDir * aiSettings.detectionRadius);
                Gizmos.DrawLine(transform.position, transform.position + rightDir * aiSettings.detectionRadius);
                
                // Draw arc
                int segments = 20;
                float angleStep = aiSettings.detectionAngle / segments;
                Vector3 previousPoint = transform.position + leftDir * aiSettings.detectionRadius;
                
                for (int i = 1; i <= segments; i++) {
                    float angle = -halfAngle + (angleStep * i);
                    var dir = Quaternion.Euler(0, angle, 0) * transform.forward;
                    var currentPoint = transform.position + dir * aiSettings.detectionRadius;
                    
                    Gizmos.DrawLine(previousPoint, currentPoint);
                    previousPoint = currentPoint;
                }
            }
            
            // Draw patrol radius
            if (_currentState == EnemyAIState.Patrol) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_patrolCenter, aiSettings.patrolRadius);
            }
            
            // Draw target line
            if (enemy != null && enemy.HasTarget) {
                Gizmos.color = Color.red;
                var targetPos = enemy.LastKnownTargetPosition;
                Gizmos.DrawLine(transform.position, targetPos);
            }
        }
        
        #endregion
    }
}
