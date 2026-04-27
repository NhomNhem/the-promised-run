using UnityEngine;
using System.Linq;
using RaycastPro.Detectors;
using ThePromisedRun.Core.FSM;
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
                new FuncPredicate(() => Input.IsJumpPressed && IsGrounded && !IsOverloaded));
            _stateMachine.AddTransition(jump, land, new FuncPredicate(() => jump.CanLand && !IsOverloaded));
            _stateMachine.AddTransition(land, locomotion, new FuncPredicate(() => land.IsLandingComplete));
            _stateMachine.AddAnyTransition(overload,
                new FuncPredicate(() => ChaosMeter >= maxChaosThreshold && CooldownTimer <= 0));
            _stateMachine.AddTransition(overload, locomotion,
                new FuncPredicate(() => OverloadTimer <= 0 && IsGrounded));

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

        #endregion

        #region Public Actions

        public void ApplyMovement() {
            Vector3 moveDir = new Vector3(Input.MoveInput.x, 0f, Input.MoveInput.y);

            Rb.linearVelocity = new Vector3(
                moveDir.x * moveSpeed,
                Rb.linearVelocity.y,
                moveDir.z * moveSpeed);

            // Only rotate visual when there's meaningful input (dead zone 0.1)
            if (moveDir.sqrMagnitude > 0.1f && visual != null) {
                Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                visual.rotation = Quaternion.Slerp(
                    visual.rotation, targetRot, rotationSpeed * Time.deltaTime);
            } else if (moveDir.sqrMagnitude <= 0.01f) {
                // Fully stopped — zero out horizontal velocity cleanly
                Rb.linearVelocity = new Vector3(0f, Rb.linearVelocity.y, 0f);
            }
        }

        public void ApplyJump() {
            Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, jumpForce, 0f);
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

        #region IDamageable

        public void TakeDamage(float amount, DamageInfo info) {
            AddChaos(amount * 0.5f, ChaosSource.Damage);
        }

        #endregion
    }
}