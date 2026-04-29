using UnityEngine;
using UnityEngine.AI;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Enemy.AI.ScriptableObjects;

namespace ThePromisedRun.Gameplay.Enemy.AI {
    /// <summary>
    /// NavMesh-based movement controller for Enemy AI
    /// Handles pathfinding, movement, and navigation using Unity's NavMesh system
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAINavMeshController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private EnemyAISettings aiSettings;
        [SerializeField] private Enemy enemy;
        [SerializeField] private EnemyAIController aiController;
        
        [Header("Components")]
        [SerializeField] private NavMeshAgent navMeshAgent;
        
        // Internal state
        private Vector3 _targetPosition;
        private bool _hasTarget;
        private float _lastNavMeshUpdate;
        private Vector3 _lastKnownTargetPosition;
        private float _timeSinceLastSeenTarget;
        
        // Properties
        public NavMeshAgent NavMeshAgent => navMeshAgent;
        public bool HasTarget => _hasTarget;
        public Vector3 TargetPosition => _targetPosition;
        public Vector3 LastKnownTargetPosition => _lastKnownTargetPosition;
        public float TimeSinceLastSeenTarget => _timeSinceLastSeenTarget;
        
        private void Awake() {
            // Get components if not assigned
            if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
            if (enemy == null) enemy = GetComponent<Enemy>();
            if (aiController == null) aiController = GetComponent<EnemyAIController>();
            if (aiSettings == null) {
                Debug.LogError("EnemyAINavMeshController: AI Settings not assigned!");
                return;
            }
            
            SetupNavMeshAgent();
        }
        
        private void SetupNavMeshAgent() {
            if (navMeshAgent == null) return;
            
            // Configure NavMeshAgent based on AI settings
            navMeshAgent.speed = aiSettings.moveSpeed;
            navMeshAgent.angularSpeed = aiSettings.rotationSpeed;
            navMeshAgent.acceleration = aiSettings.acceleration;
            navMeshAgent.stoppingDistance = aiSettings.navMeshStoppingDistance;
            navMeshAgent.autoBraking = true;
            
            // Set destination update interval
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;
            navMeshAgent.updateUpAxis = true;
        }
        
        private void Update() {
            if (aiSettings == null || !aiSettings.useNavMesh) return;
            
            UpdateTargetTracking();
            UpdateNavMeshMovement();
        }
        
        private void UpdateTargetTracking() {
            if (enemy == null || !enemy.HasTarget) {
                if (_hasTarget) {
                    _hasTarget = false;
                    _timeSinceLastSeenTarget += Time.deltaTime;
                }
                return;
            }
            
            var target = enemy.CurrentTarget;
            if (target == null) {
                _hasTarget = false;
                _timeSinceLastSeenTarget += Time.deltaTime;
                return;
            }
            
            // Update target position
            _targetPosition = ((MonoBehaviour)target).transform.position;
            _lastKnownTargetPosition = _targetPosition;
            _hasTarget = true;
            _timeSinceLastSeenTarget = 0f;
        }
        
        private void UpdateNavMeshMovement() {
            if (navMeshAgent == null || !_hasTarget) return;
            
            // Update destination at specified interval
            if (Time.time - _lastNavMeshUpdate >= aiSettings.navMeshUpdateInterval) {
                navMeshAgent.SetDestination(_targetPosition);
                _lastNavMeshUpdate = Time.time;
            }
        }
        
        #region Public Movement Methods
        
        /// <summary>
        /// Move towards a specific position using NavMesh
        /// </summary>
        public void MoveTowards(Vector3 position) {
            if (navMeshAgent == null || !aiSettings.useNavMesh) return;
            
            _targetPosition = position;
            _hasTarget = true;
            _lastKnownTargetPosition = position;
            _timeSinceLastSeenTarget = 0f;
            
            navMeshAgent.SetDestination(position);
            navMeshAgent.isStopped = false;
        }
        
        /// <summary>
        /// Stop all movement
        /// </summary>
        public void StopMovement() {
            if (navMeshAgent == null) return;
            
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            _hasTarget = false;
        }
        
        /// <summary>
        /// Face towards a target
        /// </summary>
        public void FaceTarget(IDamageable target) {
            if (target == null) return;
            
            var targetTransform = ((MonoBehaviour)target).transform;
            var direction = (targetTransform.position - transform.position).normalized;
            
            if (direction != Vector3.zero) {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        /// <summary>
        /// Check if target is in attack range
        /// </summary>
        public bool IsTargetInRange(IDamageable target) {
            if (target == null || enemy == null) return false;
            
            var distance = Vector3.Distance(transform.position, ((MonoBehaviour)target).transform.position);
            return distance <= enemy.AttackRange;
        }
        
        /// <summary>
        /// Check if should attack based on distance and angle
        /// </summary>
        public bool ShouldAttack() {
            if (!_hasTarget || enemy == null) return false;
            
            if (!IsTargetInRange(enemy.CurrentTarget)) return false;
            
            // Check if facing target (for melee attacks)
            var direction = (_targetPosition - transform.position).normalized;
            var angle = Vector3.Angle(transform.forward, direction);
            
            return angle < 45f; // Attack if facing target within 45 degrees
        }
        
        #endregion
        
        #region Patrol Methods
        
        /// <summary>
        /// Start patrolling around a center point
        /// </summary>
        public void StartPatrol(Vector3 center) {
            if (!aiSettings.useNavMesh) return;
            
            // Generate random patrol point within radius
            var randomDirection = Random.insideUnitSphere * aiSettings.patrolRadius;
            randomDirection.y = 0; // Keep on same level
            
            var patrolPoint = center + randomDirection;
            
            // Ensure point is on NavMesh
            if (NavMesh.SamplePosition(patrolPoint, out NavMeshHit hit, aiSettings.patrolRadius, NavMesh.AllAreas)) {
                MoveTowards(hit.position);
            }
        }
        
        #endregion
        
        #region Debug Methods
        
        private void OnDrawGizmosSelected() {
            if (aiSettings == null || !aiSettings.showDebugInfo) return;
            
            // Draw detection radius
            if (aiSettings.showDetectionGizmos) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, aiSettings.detectionRadius);
            }
            
            // Draw patrol radius
            if (aiSettings.showPatrolPath) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, aiSettings.patrolRadius);
            }
            
            // Draw target line
            if (_hasTarget) {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _targetPosition);
            }
        }
        
        #endregion
    }
}
