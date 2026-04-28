using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using RaycastPro.Detectors;
using ThePromisedRun.Core.FSM;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay.Juice;
using ThePromisedRun.Gameplay.States;
using ThePromisedRun.Gameplay.Input;
using UnityEngine.Events;

namespace ThePromisedRun.Gameplay {
    public class PlayerController : MonoBehaviour, IDamageable {
        #region Inspector Fields

        [Header("Movement Settings")] [SerializeField]
        private float moveSpeed = 8f;

        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;

        [Header("System Overload Settings")] [SerializeField]
        private float overloadDuration = 3f;

        [SerializeField] private float overloadCooldown = 5f;
        [SerializeField] private float maxChaosThreshold = 100f;
        [SerializeField] private float chaosDecayRate = 10f;

        [Header("Attack Settings")] [SerializeField]
        private float comboWindow = 0.6f;

        [SerializeField] private float attackCooldown = 0.15f;
        [SerializeField] private float chaosPerHit = 15f;

        [Header("References")] [SerializeField]
        private Transform visual;

        [SerializeField] private Transform detector;
        [SerializeField] private RangeDetector groundDetector;
        [SerializeField] private AttackHitbox attackHitbox;

        [Header("RaycastPro Detection")]
        [SerializeField] private RangeDetector enemyDetector;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float enemyDetectionRadius = 10f;

        #endregion

        #region Public Properties

        public Rigidbody Rb { get; private set; }
        public Animator Anim { get; private set; }
        public InputReader Input { get; private set; }
        public PlayerJuice Juice { get; private set; }
        public Transform Visual => visual;
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float RotationSpeed => rotationSpeed;
        public bool IsGrounded { get; private set; }
        public float OverloadTimer { get; private set; }
        public float CooldownTimer { get; private set; }
        public float ChaosMeter { get; private set; }
        public bool IsOverloaded => OverloadTimer > 0f;
        public bool IsAlive => true; // Player is always alive

        #endregion

        #region Events

        public UnityEvent<float> OnChaosChanged = new UnityEvent<float>();
        public UnityEvent OnOverloadStarted = new UnityEvent();
        public UnityEvent OnOverloadEnded = new UnityEvent();

        #endregion

        #region Private Fields

        private StateMachine _stateMachine;
        private int _groundContacts;

        // Attack combo
        private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
        private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
        private int _comboIndex; // 0 = idle, 1-3 = active step
        private float _comboTimer; // countdown to reset combo
        private float _attackCooldown; // min time between hits

        #endregion

        #region Unity Lifecycle

        private void Awake() {
            Rb = GetComponent<Rigidbody>();
            Input = GetComponent<InputReader>();
            Juice = GetComponent<PlayerJuice>();

            Anim = visual != null
                ? visual.GetComponent<Animator>()
                : GetComponentInChildren<Animator>();

            if (detector == null) {
                detector = Enumerable.Range(0, transform.childCount)
                    .Select(i => transform.GetChild(i))
                    .FirstOrDefault(c => c.name == "Detector");
            }

            if (detector != null)
                groundDetector = detector.GetComponentInChildren<RangeDetector>();

            _stateMachine = new StateMachine();
            SetupStateMachine();
        }

        private void Update() {
            _stateMachine.Update();
            HandleTimers();
            HandleAttack();
            CheckGround();
        }

        private void LateUpdate() {
            // Disabled visual rotation reset to prevent spinning issues
            // The Animator should handle visual rotation properly
        }

        private void FixedUpdate() {
            _stateMachine.FixedUpdate();

            // Extra gravity for snappier fall arc
            if (Rb.linearVelocity.y < 0f)
                Rb.linearVelocity +=
                    Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        #endregion

        #region FSM Setup

        private void SetupStateMachine() {
            var locomotion = new LocomotionState(this, Anim);
            var jump = new JumpState(this, Anim);
            var land = new LandState(this, Anim);
            var overload = new OverloadState(this, Anim);

            _stateMachine.AddTransition(locomotion, jump,
                new FuncPredicate(() => Input.IsJumpPressed && IsGrounded));
            _stateMachine.AddTransition(jump, land, new FuncPredicate(() => jump.CanLand));
            _stateMachine.AddTransition(land, locomotion, new FuncPredicate(() => land.IsLandingComplete));
            _stateMachine.AddAnyTransition(overload, new FuncPredicate(() => ChaosMeter >= maxChaosThreshold && CooldownTimer <= 0));
            _stateMachine.AddTransition(overload, locomotion, new FuncPredicate(() => OverloadTimer <= 0 && IsGrounded));

            _stateMachine.SetState(locomotion);
        }

        #endregion

        #region Private Handlers

        private void HandleTimers() {
            if (OverloadTimer > 0f) OverloadTimer -= Time.deltaTime;
            if (CooldownTimer > 0f) CooldownTimer -= Time.deltaTime;

            if (!IsOverloaded && ChaosMeter > 0f) {
                ChaosMeter = Mathf.Max(0f, ChaosMeter - chaosDecayRate * Time.deltaTime);
                OnChaosChanged.Invoke(ChaosMeter / maxChaosThreshold);
            }
        }

        /// <summary>
        /// Combo attack system — runs on Attack animator layer (upper body mask).
        /// Player can attack while moving/jumping without interrupting locomotion.
        /// Combo: 1 → 2 → 3 within comboWindow, resets on timeout.
        /// </summary>
        private void HandleAttack() {
            if (_attackCooldown > 0f) {
                _attackCooldown -= Time.deltaTime;
                return;
            }

            if (!Input.IsAttackPressed) {
                // Tick combo window down
                if (_comboTimer > 0f) {
                    _comboTimer -= Time.deltaTime;
                    if (_comboTimer <= 0f) ResetCombo();
                }

                return;
            }

            // Consume input
            Input.ConsumeAttackInput();
            _attackCooldown = attackCooldown;

            // Advance combo: 0→1, 1→2, 2→3, 3→1 (loop)
            _comboIndex = _comboIndex >= 3 ? 1 : _comboIndex + 1;
            _comboTimer = comboWindow;

            // Drive animator on Attack layer
            Anim.SetInteger(ComboIndexHash, _comboIndex);
            Anim.SetTrigger(AttackTriggerHash);

            // Hitbox activation is now handled by animation events
            // attackHitbox?.Activate();
            
            // Juice + chaos
            Juice?.OnAttackSwing();
            AddChaos(chaosPerHit, ChaosSource.Attack);
        }

        private void ResetCombo() {
            _comboIndex = 0;
            Anim.SetInteger(ComboIndexHash, 0);
        }

        private void CheckGround() {
            IsGrounded = groundDetector != null
                ? groundDetector.Performed
                : _groundContacts > 0;
        }

        /// <summary>
        /// Enhanced ground detection using RaycastPro RangeDetector
        /// More precise than simple collider detection
        /// </summary>
        private bool CheckGroundWithRaycastPro() {
            if (groundDetector == null) return false;
            
            // RaycastPro RangeDetector provides more accurate ground detection
            bool isGrounded = groundDetector.Performed;
            
            // Additional validation: check if detected colliders are actually ground
            if (isGrounded && groundDetector.DetectedColliders.Count > 0) {
                foreach (var collider in groundDetector.DetectedColliders) {
                    if (collider != null && IsGroundLayer(collider.gameObject.layer)) {
                        return true;
                    }
                }
                return false; // Detected but not ground layer
            }
            
            return isGrounded;
        }

        /// <summary>
        /// Check if a layer is considered ground
        /// </summary>
        private bool IsGroundLayer(int layer) {
            return layer == LayerMask.NameToLayer("Default") || 
                   layer == LayerMask.NameToLayer("Ground") ||
                   layer == LayerMask.NameToLayer("Floor");
        }

        /// <summary>
        /// Get nearest enemy using RaycastPro RangeDetector
        /// </summary>
        public GameObject GetNearestEnemy() {
            if (enemyDetector == null || !enemyDetector.Performed) return null;
            
            Collider nearest = enemyDetector.NearestMember;
            if (nearest != null && IsEnemyLayer(nearest.gameObject.layer)) {
                return nearest.gameObject;
            }
            
            // Fallback: check all detected colliders for nearest enemy
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var collider in enemyDetector.DetectedColliders) {
                if (collider != null && IsEnemyLayer(collider.gameObject.layer)) {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < nearestDistance) {
                        nearestDistance = distance;
                        nearestEnemy = collider.gameObject;
                    }
                }
            }
            
            return nearestEnemy;
        }

        /// <summary>
        /// Check if a layer is considered enemy
        /// </summary>
        private bool IsEnemyLayer(int layer) {
            return ((1 << layer) & enemyLayer) != 0;
        }

        /// <summary>
        /// Get all enemies in detection range
        /// </summary>
        public GameObject[] GetEnemiesInRange() {
            if (enemyDetector == null || !enemyDetector.Performed) 
                return new GameObject[0];
            
            var enemies = new List<GameObject>();
            
            foreach (var collider in enemyDetector.DetectedColliders) {
                if (collider != null && IsEnemyLayer(collider.gameObject.layer)) {
                    enemies.Add(collider.gameObject);
                }
            }
            
            return enemies.ToArray();
        }

        /// <summary>
        /// Check if player is surrounded by enemies
        /// </summary>
        public bool IsSurrounded() {
            var enemies = GetEnemiesInRange();
            if (enemies.Length < 3) return false; // Need at least 3 enemies to be surrounded
            
            int enemiesInFront = 0;
            int enemiesBehind = 0;
            
            foreach (var enemy in enemies) {
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, toEnemy);
                
                if (dotProduct > 0.5f) enemiesInFront++;
                else if (dotProduct < -0.5f) enemiesBehind++;
            }
            
            return enemiesInFront >= 1 && enemiesBehind >= 1;
        }

        public void ApplyMovement() {
            Vector3 moveDir = new Vector3(Input.MoveInput.x, 0f, Input.MoveInput.y);

            Rb.linearVelocity = new Vector3(
                moveDir.x * moveSpeed,
                Rb.linearVelocity.y,
                moveDir.z * moveSpeed);

            // Only rotate when actually moving to prevent spinning
            if (moveDir.sqrMagnitude > 0.01f) {
                Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        public void ApplyJump() {
            Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, jumpForce, Rb.linearVelocity.z);
            Input.ConsumeJumpInput();
            AddChaos(20f, ChaosSource.Jump);
        }

        public void AddChaos(float amount, ChaosSource source = ChaosSource.Manual) {
            if (IsOverloaded) return;
            ChaosMeter = Mathf.Min(ChaosMeter + amount, maxChaosThreshold);
            OnChaosChanged.Invoke(ChaosMeter / maxChaosThreshold);
        }

        public void InitiateOverload() {
            OverloadTimer = overloadDuration;
            CooldownTimer = overloadCooldown;
            ChaosMeter = 0f;
            Juice?.OnOverloadStart();
            OnOverloadStarted.Invoke();
        }

        public void EndOverload() {
            Juice?.OnOverloadEnd();
            OnOverloadEnded.Invoke();
        }

        #endregion

        #region Animation Event Receivers

        /// <summary>
        /// Called by animation event to activate attack hitbox
        /// </summary>
        public void OnHitboxActivate() {
            attackHitbox?.Activate();
        }

        /// <summary>
        /// Called by animation event to deactivate attack hitbox
        /// </summary>
        public void OnHitboxDeactivate() {
            attackHitbox?.Deactivate();
        }

        #endregion

        #region IDamageable

        public float Health => 100f; // Player has full health system in PlayerHealth component
        public float MaxHealth => 100f;
        public System.Action<float> OnHealthChanged { get; set; } = (health) => { };
        public System.Action OnDeath { get; set; } = () => { };

        public void TakeDamage(float amount, DamageInfo info) {
            AddChaos(amount * 0.5f, ChaosSource.Damage);
        }

        #endregion
    }
}