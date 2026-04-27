using UnityEngine;
using UnityEngine.Events;
using ThePromisedRun.Core;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Juice;
using ThePromisedRun.Gameplay.Input;

namespace ThePromisedRun.Gameplay {
    /// <summary>
    /// Player entity implementing SOLID principles
    /// SOLID: Single Responsibility - Manages player-specific logic only
    /// </summary>
    public class Player : Entity, IMovable, IAttacker {
        [Header("Player Stats")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        
        [Header("Combat Stats")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 0.15f;
        
        [Header("Chaos System")]
        [SerializeField] private float maxChaosThreshold = 100f;
        [SerializeField] private float chaosDecayRate = 10f;
        [SerializeField] private float overloadDuration = 3f;
        [SerializeField] private float overloadCooldown = 5f;
        
        [Header("References")]
        [SerializeField] private PlayerJuice juice;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Animator animator;
        [SerializeField] private InputReader input;
        
        // Events
        public UnityEvent<float> OnChaosChanged = new UnityEvent<float>();
        public UnityEvent OnOverloadStarted = new UnityEvent();
        public UnityEvent OnOverloadEnded = new UnityEvent();
        
        // Private state
        private float _chaosMeter;
        private float _overloadTimer;
        private float _cooldownTimer;
        private float _attackCooldownTimer;
        private bool _isGrounded;
        
        // IMovable Implementation
        public float MoveSpeed { 
            get => moveSpeed; 
            set => moveSpeed = value; 
        }
        
        public float JumpForce { 
            get => jumpForce; 
            set => jumpForce = value; 
        }
        
        public bool IsGrounded => _isGrounded;
        
        // IAttacker Implementation
        public float BaseDamage { 
            get => baseDamage; 
            set => baseDamage = value; 
        }
        
        public float AttackRange { 
            get => attackRange; 
            set => attackRange = value; 
        }
        
        public float AttackCooldown => _attackCooldownTimer;
        
        public bool CanAttack => _attackCooldownTimer <= 0f && IsAlive;
        
        // Events
        public System.Action OnMoveStart { get; set; }
        public System.Action OnMoveStop { get; set; }
        public System.Action OnJump { get; set; }
        public System.Action OnLand { get; set; }
        public System.Action OnAttackStart { get; set; }
        public System.Action<IDamageable> OnAttackHit { get; set; }
        public System.Action OnAttackEnd { get; set; }
        
        public float ChaosMeter => _chaosMeter;
        public float OverloadTimer => _overloadTimer;
        public float CooldownTimer => _cooldownTimer;
        public bool IsOverloaded => _overloadTimer > 0f;
        
        protected override void Awake() {
            base.Awake();
            
            // Initialize events
            OnMoveStart = () => { };
            OnMoveStop = () => { };
            OnJump = () => { juice?.OnTakeoff(); };
            OnLand = () => { juice?.OnLand(); };
            OnAttackStart = () => { juice?.OnAttackSwing(); };
            OnAttackHit = (target) => { juice?.OnAttackHit(); };
            OnAttackEnd = () => { };
            
            // Initialize chaos system
            _chaosMeter = 0f;
            _overloadTimer = 0f;
            _cooldownTimer = 0f;
            _attackCooldownTimer = 0f;
        }
        
        private void Update() {
            HandleTimers();
            HandleChaosDecay();
        }
        
        private void FixedUpdate() {
            ApplyExtraGravity();
        }
        
        #region IMovable Implementation
        public void Move(Vector3 direction, float speed = -1f) {
            if (!IsAlive) return;
            
            float actualSpeed = speed > 0 ? speed : moveSpeed;
            Vector3 moveVelocity = direction * actualSpeed;
            
            rb.linearVelocity = new Vector3(
                moveVelocity.x,
                rb.linearVelocity.y,
                moveVelocity.z
            );
            
            // Rotate to face movement direction
            if (direction.sqrMagnitude > 0.01f) {
                Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
            
            OnMoveStart?.Invoke();
        }
        
        public void Jump(float force = -1f) {
            if (!IsAlive || !_isGrounded) return;
            
            float actualForce = force > 0 ? force : jumpForce;
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                actualForce,
                rb.linearVelocity.z
            );
            
            OnJump?.Invoke();
            AddChaos(20f);
        }
        
        public void Stop() {
            if (!IsAlive) return;
            
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            OnMoveStop?.Invoke();
        }
        #endregion
        
        #region IAttacker Implementation
        public void Attack() {
            if (!CanAttack) return;
            
            _attackCooldownTimer = attackCooldown;
            
            // Trigger attack animation
            animator.SetTrigger("AttackTrigger");
            
            OnAttackStart?.Invoke();
            
            // Attack hitbox will be activated by animation events
        }
        
        public bool IsTargetInRange(IDamageable target) {
            if (target == null) return false;
            
            float distance = Vector3.Distance(transform.position, ((MonoBehaviour)target).transform.position);
            return distance <= attackRange;
        }
        #endregion
        
        #region Player-Specific Methods
        /// <summary>
        /// Add chaos to the chaos meter
        /// </summary>
        public void AddChaos(float amount) {
            if (IsOverloaded) return;
            
            _chaosMeter = Mathf.Min(_chaosMeter + amount, maxChaosThreshold);
            OnChaosChanged.Invoke(_chaosMeter / maxChaosThreshold);
            
            // Check for overload
            if (_chaosMeter >= maxChaosThreshold && _cooldownTimer <= 0f) {
                InitiateOverload();
            }
        }
        
        /// <summary>
        /// Initiate overload state
        /// </summary>
        public void InitiateOverload() {
            _overloadTimer = overloadDuration;
            _cooldownTimer = overloadCooldown;
            _chaosMeter = 0f;
            
            Debug.Log("[Player] Overload initiated!");
            juice?.OnOverloadStart();
            OnOverloadStarted.Invoke();
        }
        
        /// <summary>
        /// End overload state
        /// </summary>
        public void EndOverload() {
            juice?.OnOverloadEnd();
            OnOverloadEnded.Invoke();
        }
        
        /// <summary>
        /// Animation event: Activate attack hitbox
        /// </summary>
        public void OnHitboxActivate() {
            // This will be called by animation events
            // AttackHitbox component will handle the actual activation
        }
        
        /// <summary>
        /// Animation event: Deactivate attack hitbox
        /// </summary>
        public void OnHitboxDeactivate() {
            // This will be called by animation events
            // AttackHitbox component will handle the actual deactivation
        }
        
        /// <summary>
        /// Set grounded state
        /// </summary>
        public void SetGrounded(bool grounded) {
            if (_isGrounded != grounded) {
                _isGrounded = grounded;
                if (grounded && rb.linearVelocity.y < 0) {
                    OnLand?.Invoke();
                }
            }
        }
        #endregion
        
        #region Private Methods
        private void HandleTimers() {
            if (_overloadTimer > 0f) _overloadTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;
        }
        
        private void HandleChaosDecay() {
            if (!IsOverloaded && _chaosMeter > 0f) {
                _chaosMeter = Mathf.Max(0f, _chaosMeter - chaosDecayRate * Time.deltaTime);
                OnChaosChanged.Invoke(_chaosMeter / maxChaosThreshold);
            }
        }
        
        private void ApplyExtraGravity() {
            if (rb.linearVelocity.y < 0f) {
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }
        #endregion
        
        #region Entity Overrides
        protected override void HandleDeath() {
            Debug.Log("[Player] Player died!");
            // Disable player controls
            enabled = false;
            rb.isKinematic = true;
            
            // Play death animation
            animator.SetTrigger("Death");
        }
        
        protected override void OnRevive() {
            Debug.Log("[Player] Player revived!");
            // Enable player controls
            enabled = true;
            rb.isKinematic = false;
            
            // Reset animation state
            animator.SetTrigger("Revive");
        }
        #endregion
    }
}
