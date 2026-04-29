using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using ThePromisedRun.Core;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Core.Systems;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay.Enemy.ScriptableObjects;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Enemy entity following SOLID principles
    /// SOLID: Single Responsibility - Only manages enemy entity state and basic capabilities
    /// SOLID: Open/Closed - Can be extended without modification
    /// SOLID: Interface Segregation - Implements only relevant interfaces
    /// </summary>
    [DisallowMultipleComponent]
    public class Enemy : Entity, IEnemyEntity, IAttacker {
        [Header("Enemy Properties")]
        [SerializeField] protected EnemyProperties enemyProperties;
        
        [Header("Enemy Stats")]
        [SerializeField] protected float baseDamage = 15f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackCooldown = 1f;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float rotationSpeed = 120f;
        
        [Header("Components")]
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected Animator animator;
        [SerializeField] protected Transform visual;

        // Cached NavMeshAgent — used when present, falls back to Rigidbody movement
        private UnityEngine.AI.NavMeshAgent _navAgent;
        
        [Header("Detection")]
        [SerializeField] protected LayerMask targetLayers;
        [SerializeField] protected float detectionRadius = 10f;
        [SerializeField] protected float loseTargetTime = 5f;
        
        // Events
        public UnityEvent<IDamageable> OnTargetAcquired = new UnityEvent<IDamageable>();
        public UnityEvent<IDamageable> OnTargetLost = new UnityEvent<IDamageable>();
        public UnityEvent OnAttackStarted = new UnityEvent();
        public UnityEvent OnAttackCompleted = new UnityEvent();
        
        // IEnemyEntity Implementation
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        // AttackRange and BaseDamage are handled by IAttacker implementation below
        public bool CanAttack => attackCooldownTimer <= 0f && IsAlive && HasTarget && IsTargetInRange(CurrentTarget);
        public float DetectionRadius => detectionRadius;
        public GameObject GameObject => gameObject;
        
        // IAttacker Implementation
        public float BaseDamage { 
            get => baseDamage; 
            set => baseDamage = value; 
        }
        
        public float AttackRange { 
            get => attackRange; 
            set => attackRange = value; 
        }
        
        public float AttackCooldown => attackCooldownTimer;
        
        // Events
        public System.Action OnAttackStart { get; set; }
        public System.Action<IDamageable> OnAttackHit { get; set; }
        public System.Action OnAttackEnd { get; set; }
        
        // Protected state
        protected IDamageable currentTarget;
        protected Vector3 lastKnownTargetPosition;
        protected float timeSinceLastSeenTarget;
        protected bool hasTarget;
        protected float attackCooldownTimer;
        protected bool isAttacking;
        
        // Public Properties
        public IDamageable CurrentTarget => currentTarget;
        public bool HasTarget => hasTarget;
        public Vector3 LastKnownTargetPosition => lastKnownTargetPosition;
        public float TimeSinceLastSeenTarget => timeSinceLastSeenTarget;
        public bool IsAttacking => isAttacking;
        /// <summary>Exposes the cached Animator for FSM states.</summary>
        public Animator Animator => animator;

        /// <summary>Resets attack cooldown timer — called by EnemyAttackFSMState.OnExit().</summary>
        public void ResetAttackCooldown() {
            attackCooldownTimer = attackCooldown;
        }
        
        protected override void Awake() {
            base.Awake();
            
            // Initialize events
            OnAttackStart = () => { };
            OnAttackHit = (target) => { };
            OnAttackEnd = () => { };
            
            // Initialize state
            attackCooldownTimer = 0f;
            timeSinceLastSeenTarget = 0f;
            hasTarget = false;
            isAttacking = false;
            
            // Find components
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (visual == null) visual = GetComponentInChildren<Transform>();

            // Ensure animator is enabled and ready to play
            if (animator != null) {
                animator.enabled = true;
                try {
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                } catch {
                    // Some Unity versions or animator setups may not allow changing culling mode at runtime; ignore safely
                }
                if (animator.runtimeAnimatorController == null) {
                    Debug.LogWarning($"[Enemy] Animator found on {gameObject.name} but no RuntimeAnimatorController is assigned. Animations will not play.");
                }
            }
            
            // Cache NavMeshAgent if present — used for pathfinding movement
            _navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (_navAgent != null) {
                Debug.Log($"[Enemy] NavMeshAgent found on {gameObject.name} - isOnNavMesh={_navAgent.isOnNavMesh}");

                // If agent isn't on the NavMesh, try to warp it to nearest NavMesh point.
                if (!_navAgent.isOnNavMesh) {
                    NavMeshHit hit;
                    bool found = NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas);
                    if (found) {
                        _navAgent.Warp(hit.position);
                        Debug.Log($"[Enemy] Warped NavMeshAgent to nearest NavMesh at {hit.position}");
                    } else {
                        // No NavMesh nearby — fall back to Rigidbody movement by keeping physics enabled
                        Debug.LogWarning($"[Enemy] NavMeshAgent present but not on NavMesh and no nearby NavMesh found for {gameObject.name}. Falling back to Rigidbody.");
                        if (rb != null) {
                            rb.isKinematic = false;
                            rb.useGravity = true;
                        }
                        _navAgent = null; // disable agent usage at runtime
                    }
                } else {
                    // EnemyBrain uses direct Rigidbody movement — keep physics enabled
                    // NavMeshAgent is kept for obstacle avoidance only
                    if (rb != null) {
                        rb.isKinematic = false;
                        rb.useGravity  = true;
                    }
                }
            }

            // Runtime debug summary for scene checks
            string navStatus = _navAgent != null ? (_navAgent.isOnNavMesh ? "OnNavMesh" : "NotOnNavMesh") : "NoAgent";
            Debug.Log($"[Enemy][RuntimeSummary] {gameObject.name} - nav={navStatus} rbPresent={(rb != null)} detectionRadius={detectionRadius} loseTargetTime={loseTargetTime}");
        }
        
        public virtual void Update() {
            if (!IsAlive) return;
            
            HandleTimers();
            UpdateTargetTracking();
        }
        
        #region IEnemyEntity Implementation
        public void MoveTowards(Vector3 position) {
            if (!IsAlive) return;

            if (_navAgent != null && _navAgent.isOnNavMesh) {
                // NavMesh pathfinding
                _navAgent.isStopped = false;
                _navAgent.SetDestination(position);
            } else {
                string isOnNav = _navAgent != null ? _navAgent.isOnNavMesh.ToString() : "N/A";
                // Fallback: direct Rigidbody movement
                Vector3 direction = (position - transform.position).normalized;
                rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);

                if (direction.sqrMagnitude > 0.01f) {
                    Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }
        }
        
        public void StopMovement() {
            if (!IsAlive) return;

            if (_navAgent != null && _navAgent.isOnNavMesh) {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
            } else if (rb != null) {
                rb.linearVelocity = Vector3.zero;
            }
        }
        
        public void FaceTarget(IDamageable target) {
            if (!IsAlive || target == null) return;
            
            Vector3 direction = ((MonoBehaviour)target).transform.position - transform.position;
            direction.y = 0f;
            
            if (direction.sqrMagnitude > 0.01f) {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        public void Attack() {
            if (!CanAttack) return;
            
            isAttacking = true;
            attackCooldownTimer = attackCooldown;
            
            // Trigger attack animation
            if (animator != null) {
                animator.SetTrigger("Attack");
            } else {
                Debug.LogWarning($"[Enemy] Animator missing on {gameObject.name} - attack animation will not play. Falling back to event-driven hitbox activation.");
            }
            
            OnAttackStarted?.Invoke();
        }
        
        public void SetTarget(IDamageable target) {
            if (target == currentTarget) return; // Already targeting
            
            // Lost old target event
            if (currentTarget != null) {
                OnTargetLost?.Invoke(currentTarget);
            }
            
            // Acquire new target
            currentTarget = target;
            hasTarget = true;
            timeSinceLastSeenTarget = 0f;
            lastKnownTargetPosition = ((MonoBehaviour)target).transform.position;
            
            OnTargetAcquired?.Invoke(target);
        }
        
        public void ClearTarget() {
            if (currentTarget != null) {
                OnTargetLost?.Invoke(currentTarget);
            }
            
            currentTarget = null;
            hasTarget = false;
            timeSinceLastSeenTarget = 0f;
            lastKnownTargetPosition = Vector3.zero;
            
            OnTargetLost?.Invoke(null);
        }
        
        public bool IsTargetInRange(IDamageable target) {
            if (target == null) return false;
            
            float distance = Vector3.Distance(transform.position, ((MonoBehaviour)target).transform.position);
            return distance <= attackRange;
        }
        #endregion
        
        #region Protected Methods
        protected virtual void UpdateTargetTracking() {
            if (!hasTarget || currentTarget == null) return;
            
            // Check if target is still alive
            if (!currentTarget.IsAlive) {
                ClearTarget();
                return;
            }
            
            // Update last known position only when target is within detection radius
            var targetPos = ((MonoBehaviour)currentTarget).transform.position;
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance <= detectionRadius) {
                // Target is still in detection range: refresh last known position and reset timer
                lastKnownTargetPosition = targetPos;
                timeSinceLastSeenTarget = 0f;
            } else {
                // Target is out of detection range: accumulate time since last seen
                timeSinceLastSeenTarget += Time.deltaTime;
                if (timeSinceLastSeenTarget >= loseTargetTime) {
                    ClearTarget();
                }
            }
        }
        
        protected virtual void HandleTimers() {
            if (attackCooldownTimer > 0f) {
                attackCooldownTimer -= Time.deltaTime;
            }
            
            if (isAttacking && attackCooldownTimer <= 0f) {
                isAttacking = false;
                OnAttackCompleted?.Invoke();
                OnAttackEnd?.Invoke();
            }
        }
        
        protected virtual bool IsTargetInSight(IDamageable target) {
            if (target == null) return false;
            
            Vector3 toTarget = ((MonoBehaviour)target).transform.position - transform.position;
            float distance = toTarget.magnitude;
            
            if (distance > detectionRadius) return false;
            
            // Simple line of sight check
            RaycastHit hit;
            return !Physics.Linecast(transform.position + Vector3.up, 
                ((MonoBehaviour)target).transform.position + Vector3.up, 
                out hit, targetLayers);
        }
        
        protected virtual float GetDistanceToTarget(IDamageable target) {
            if (target == null) return float.MaxValue;
            return Vector3.Distance(transform.position, ((MonoBehaviour)target).transform.position);
        }
        #endregion
        
        #region Entity Overrides
        protected override void HandleDeath() {
            Debug.Log($"[{gameObject.name}] Enemy died!");
            
            // Clear target on death
            ClearTarget();
            
            // Stop movement
            StopMovement();
            
            // Disable components
            enabled = false;
            rb.isKinematic = true;
            
            // Play death animation
            if (animator != null) {
                animator.SetTrigger("Death");
            }
            
            // Notify listeners
            OnTargetLost?.Invoke(null);
        }
        
        protected override void OnRevive() {
            Debug.Log($"[{gameObject.name}] Enemy revived!");
            
            // Re-enable components
            enabled = true;
            rb.isKinematic = false;
            
            // Reset state
            attackCooldownTimer = 0f;
            isAttacking = false;
            hasTarget = false;
            
            // Play revive animation
            if (animator != null) {
                animator.SetTrigger("Revive");
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Get damage resistance for specific damage type
        /// </summary>
        public virtual float GetDamageResistance(DamageType damageType) {
            // Default resistance - can be overridden by derived classes
            switch (damageType) {
                case DamageType.Physical:
                    return 1.0f;
                case DamageType.Magic:
                    return 0.8f;
                case DamageType.Environmental:
                    return 0.5f;
                case DamageType.Overload:
                    return 1.2f;
                default:
                    return 1.0f;
            }
        }
        
        /// <summary>
        /// Check if should attack based on distance and angle
        /// </summary>
        public virtual bool ShouldAttack() {
            if (!hasTarget || !IsTargetInRange(currentTarget)) return false;
            
            float angle = Vector3.Angle(transform.forward, 
                (((MonoBehaviour)currentTarget).transform.position - transform.position).normalized);
            
            // Attack if facing target and in range
            return angle < 45f;
        }
        #endregion
    }
}
