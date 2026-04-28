using UnityEngine;
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
        }
        
        public virtual void Update() {
            if (!IsAlive) return;
            
            HandleTimers();
            UpdateTargetTracking();
        }
        
        #region IEnemyEntity Implementation
        public void MoveTowards(Vector3 position) {
            if (!IsAlive) return;
            
            Vector3 direction = (position - transform.position).normalized;
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            
            rb.MovePosition(movement);
            
            // Rotate to face target
            if (direction.sqrMagnitude > 0.01f) {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        public void StopMovement() {
            if (!IsAlive) return;
            rb.linearVelocity = Vector3.zero;
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
            }
            
            OnAttackStarted?.Invoke();
            Debug.Log($"[{gameObject.name}] Attacking target");
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
            Debug.Log($"[{gameObject.name}] Acquired target: {target.GetType().Name}");
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
            Debug.Log($"[{gameObject.name}] Lost target");
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
            
            // Update last known position
            lastKnownTargetPosition = ((MonoBehaviour)currentTarget).transform.position;
            timeSinceLastSeenTarget = 0f;
            
            // Check if target is out of detection range
            float distance = Vector3.Distance(transform.position, lastKnownTargetPosition);
            if (distance > detectionRadius) {
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
