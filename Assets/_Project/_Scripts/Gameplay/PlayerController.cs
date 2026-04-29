using UnityEngine;
using System.Linq;
using OpenUtility.Data;
using RaycastPro.Detectors;
using ThePromisedRun.Core.FSM;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay.Juice;
using ThePromisedRun.Gameplay.States;
using ThePromisedRun.Gameplay.Input;
using ThePromisedRun.Gameplay.Player;
using ThePromisedRun.Gameplay.Player.ScriptableObjects;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ThePromisedRun.Gameplay {
    public class PlayerController : MonoBehaviour, IDamageable {
        #region Inspector Fields

        [BoxGroup("Player Properties")] [Required] [SerializeField]
        private PlayerProperties _playerProperties;

        [BoxGroup("References")] [Header("References")] [SerializeField]
        private Transform _visual;

        [BoxGroup("References")] [SerializeField]
        private Transform _detector;

        [BoxGroup("References")] [SerializeField]
        private RangeDetector _groundDetector;

        [BoxGroup("References")] [SerializeField]
        private AttackHitbox _attackHitbox;

        [BoxGroup("Detection")] [SerializeField]
        private RaycastProEnemyDetector _enemyDetector;

        [BoxGroup("ScriptableVariables")] [SerializeField]
        private ScriptableFloat _chaosMeterVar;

        #endregion

        #region Public Properties

        public Rigidbody Rb { get; private set; }
        public Animator Anim { get; private set; }
        public InputReader Input { get; private set; }
        public PlayerJuice Juice { get; private set; }
        public Transform Visual => _visual;
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public float RotationSpeed => _rotationSpeed;
        public bool IsGrounded { get; private set; }
        /// <summary>True for 8 frames after leaving ground (coyote time).</summary>
        public bool HasCoyoteTime { get; private set; }
        public bool IsDashReady => _dashCooldownTimer <= 0f;
        public float OverloadTimer { get; private set; }
        public float CooldownTimer { get; private set; }
        public float ChaosMeter { get; private set; }
        public bool IsOverloaded => OverloadTimer > 0f;
        public bool IsAlive => true;
        public float ChaosPerHit => _chaosPerHit;
        public float[] AttackStepForce => _attackStepForce;
        public float ComboHitCooldown => _comboHitCooldown;
        public float AttackExitDelay => _attackExitDelay;
        public float HitStopDuration => _hitStopDuration;
        public float HitStopTimeScale => _hitStopTimeScale;
        public float[] ComboClipDurations => _comboClipDurations;
        public float ComboChainFraction => _comboChainFraction;
        public float ComboFinishDelay => _comboFinishDelay;
        public float AttackBlendTime => _attackBlendTime;
        public float AttackMoveDamping => _attackMoveDamping;

        // Dash properties
        public bool  IsDashInvincible   { get; private set; }
        public float DashSpeed          => _dashSpeed;
        public float DashDuration       => _dashDuration;
        public float DashIFrameDuration => _dashIFrameDuration;
        public float ChaosPerDash       => _chaosPerDash;

        #endregion

        #region Events

        public UnityEvent<float> OnChaosChanged = new UnityEvent<float>();
        public UnityEvent OnOverloadStarted = new UnityEvent();
        public UnityEvent OnOverloadEnded = new UnityEvent();

        #endregion

        #region Private Fields

        private StateMachine _stateMachine;
        private int _groundContacts;

        // Backing fields loaded from PlayerProperties SO
        private float _moveSpeed;
        private float _jumpForce;
        private float _rotationSpeed;
        private float _fallGravityMultiplier;
        private float _overloadDuration;
        private float _overloadCooldown;
        private float _maxChaosThreshold;
        private float _chaosDecayRate;
        private float _comboWindow;
        private float _attackCooldown;
        private float _chaosPerHit;
        private float[] _attackStepForce;
        private float _comboHitCooldown;
        private float _attackExitDelay;
        private float _hitStopDuration;
        private float _hitStopTimeScale;
        private float[] _comboClipDurations;
        private float _comboChainFraction;
        private float _comboFinishDelay;
        private float _attackBlendTime;
        private float _attackMoveDamping;

        // Attack state reference for animation event relay
        private AttackState _attackState;
        private DashState   _dashState;

        // Dash cooldown
        private float _dashCooldownTimer;

        // Dash backing fields (loaded from PlayerProperties SO)
        private float _dashSpeed;
        private float _dashDuration;
        private float _dashIFrameDuration;
        private float _chaosPerDash;

        // Coyote time
        private bool  _wasGrounded;
        private float _coyoteTimer;
        private const float CoyoteTime = 8f / 60f; // 8 frames

        #endregion

        #region Unity Lifecycle

        private void Awake() {
            Rb = GetComponent<Rigidbody>();
            Input = GetComponent<InputReader>();
            Juice = GetComponent<PlayerJuice>();

            Anim = _visual != null
                ? _visual.GetComponentInChildren<Animator>(true)
                : GetComponentInChildren<Animator>(true);

            // Ensure Animator is enabled
            if (Anim != null) Anim.enabled = true;
            else Debug.LogError("[PlayerController] Animator not found!");

            if (_detector == null) {
                _detector = Enumerable.Range(0, transform.childCount)
                    .Select(i => transform.GetChild(i))
                    .FirstOrDefault(c => c.name == "Detector");
            }

            if (_detector != null)
                _groundDetector = _detector.GetComponentInChildren<RangeDetector>();

            LoadPlayerProperties();

            if (_enemyDetector == null)
                _enemyDetector = GetComponentInChildren<RaycastProEnemyDetector>();

            _stateMachine = new StateMachine();
            SetupStateMachine();
        }

        /// <summary>
        /// Load all player settings from PlayerProperties ScriptableObject.
        /// SOLID: Open/Closed — config loaded from SO without modifying existing code.
        /// Req 1.3, 1.4
        /// </summary>
        private void LoadPlayerProperties() {
            _moveSpeed = _playerProperties.moveSpeed;
            _jumpForce = _playerProperties.jumpForce;
            _rotationSpeed = _playerProperties.rotationSpeed;
            _fallGravityMultiplier = _playerProperties.fallGravityMultiplier;
            _overloadDuration = _playerProperties.overloadDuration;
            _overloadCooldown = _playerProperties.overloadCooldown;
            _maxChaosThreshold = _playerProperties.maxChaosThreshold;
            _chaosDecayRate = _playerProperties.chaosDecayRate;
            _comboWindow = _playerProperties.comboWindow;
            _attackCooldown = _playerProperties.attackCooldown;
            _chaosPerHit = _playerProperties.chaosPerHit;
            _attackStepForce  = _playerProperties.attackStepForce;
            _comboHitCooldown = _playerProperties.comboHitCooldown;
            _attackExitDelay  = _playerProperties.attackExitDelay;
            _hitStopDuration  = _playerProperties.hitStopDuration;
            _hitStopTimeScale = _playerProperties.hitStopTimeScale;
            _comboClipDurations  = _playerProperties.comboClipDurations;
            _comboChainFraction  = _playerProperties.comboChainFraction;
            _comboFinishDelay    = _playerProperties.comboFinishDelay;
            _attackBlendTime     = _playerProperties.attackBlendTime;
            _attackMoveDamping   = _playerProperties.attackMoveDamping;

            // Dash fields
            _dashSpeed          = _playerProperties.dashDistance / _playerProperties.dashDuration;
            _dashDuration       = _playerProperties.dashDuration;
            _dashIFrameDuration = _playerProperties.dashIFrameDuration;
            _chaosPerDash       = _playerProperties.chaosPerDash;
        }

        private void Update() {
            if (_stateMachine == null) return;
            _stateMachine.Update();
            HandleTimers();
            CheckGround();
        }

        private void FixedUpdate() {
            if (_stateMachine == null) return;
            _stateMachine.FixedUpdate();

            if (Rb.linearVelocity.y < 0f)
                Rb.linearVelocity +=
                    Vector3.up * Physics.gravity.y * (_fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        #endregion

        #region FSM Setup

        private void SetupStateMachine() {
            LocomotionState locomotion = new LocomotionState(this, Anim);
            JumpState       jump       = new JumpState(this, Anim);
            LandState       land       = new LandState(this, Anim);
            OverloadState   overload   = new OverloadState(this, Anim);
            _attackState               = new AttackState(this, Anim);
            _dashState                 = new DashState(this, Anim);
            var parryState             = new ParryState(this, Anim);

            // Locomotion ↔ Jump (coyote time + jump buffer)
            _stateMachine.AddTransition(locomotion, jump,
                new FuncPredicate(() => (Input.IsJumpPressed || Input.HasJumpBuffer)
                                        && (IsGrounded || HasCoyoteTime)));
            _stateMachine.AddTransition(jump, land, new FuncPredicate(() => jump.CanLand));
            _stateMachine.AddTransition(land, locomotion, new FuncPredicate(() => land.IsLandingComplete));

            // Dash — from Locomotion, Jump, Land (not during attack or overload)
            _stateMachine.AddTransition(locomotion, _dashState,
                new FuncPredicate(() => Input.IsDashPressed && IsDashReady && !IsOverloaded));
            _stateMachine.AddTransition(jump, _dashState,
                new FuncPredicate(() => Input.IsDashPressed && IsDashReady && !IsOverloaded));
            _stateMachine.AddTransition(land, _dashState,
                new FuncPredicate(() => Input.IsDashPressed && IsDashReady && !IsOverloaded));
            _stateMachine.AddTransition(_dashState, locomotion,
                new FuncPredicate(() => _dashState.CanExit && IsGrounded));
            _stateMachine.AddTransition(_dashState, jump,
                new FuncPredicate(() => _dashState.CanExit && !IsGrounded));

            // Parry — from Locomotion only, not during attack/dash/overload
            _stateMachine.AddTransition(locomotion, parryState,
                new FuncPredicate(() => Input.IsParryPressed && !IsOverloaded && !parryState.IsLockedOut));
            _stateMachine.AddTransition(parryState, locomotion,
                new FuncPredicate(() => parryState.CanExit));

            // Attack
            _stateMachine.AddTransition(locomotion, _attackState,
                new FuncPredicate(() => Input.IsAttackPressed && !IsOverloaded));
            _stateMachine.AddTransition(land, _attackState,
                new FuncPredicate(() => Input.IsAttackPressed && !IsOverloaded && land.IsLandingComplete));
            _stateMachine.AddTransition(_attackState, locomotion,
                new FuncPredicate(() => _attackState.CanExit));
            _stateMachine.AddTransition(_attackState, jump,
                new FuncPredicate(() => Input.IsJumpPressed && IsGrounded));

            // Overload
            _stateMachine.AddAnyTransition(overload,
                new FuncPredicate(() => ChaosMeter >= _maxChaosThreshold && CooldownTimer <= 0));
            _stateMachine.AddTransition(overload, locomotion,
                new FuncPredicate(() => OverloadTimer <= 0 && IsGrounded));

            _stateMachine.SetState(locomotion);
        }

        #endregion

        #region Private Handlers

        private void HandleTimers() {
            if (OverloadTimer > 0f) OverloadTimer -= Time.deltaTime;
            if (CooldownTimer > 0f) CooldownTimer -= Time.deltaTime;
            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;

            // Coyote time countdown
            if (_coyoteTimer > 0f) {
                _coyoteTimer -= Time.deltaTime;
                HasCoyoteTime = _coyoteTimer > 0f;
            }

            if (!IsOverloaded && ChaosMeter > 0f) {
                ChaosMeter = Mathf.Max(0f, ChaosMeter - _chaosDecayRate * Time.deltaTime);
                _chaosMeterVar?.SetValue(ChaosMeter);
                OnChaosChanged.Invoke(ChaosMeter / _maxChaosThreshold);
            }
        }

        private void CheckGround() {
            bool grounded = _groundDetector != null
                ? _groundDetector.Performed
                : _groundContacts > 0;

            // Coyote time: start countdown when leaving ground
            if (_wasGrounded && !grounded) {
                _coyoteTimer  = CoyoteTime;
                HasCoyoteTime = true;
            }
            if (grounded) {
                HasCoyoteTime = false;
                _coyoteTimer  = 0f;
            }

            _wasGrounded = grounded;
            IsGrounded   = grounded;
        }

        #endregion

        #region Enemy Detection — Delegated to IEnemyDetector

        /// <summary>
        /// Returns the nearest enemy in range. Delegates to IEnemyDetector.
        /// Req 2.4, 2.5
        /// </summary>
        public GameObject GetNearestEnemy() =>
            _enemyDetector != null ? _enemyDetector.GetNearestEnemy() : null;

        /// <summary>
        /// Returns all enemies in detection range. Delegates to IEnemyDetector.
        /// Req 2.4, 2.5
        /// </summary>
        public GameObject[] GetEnemiesInRange() =>
            _enemyDetector != null ? _enemyDetector.GetEnemiesInRange() : new GameObject[0];

        /// <summary>
        /// Returns true if player is surrounded by enemies. Delegates to IEnemyDetector.
        /// Req 2.4, 2.5
        /// </summary>
        public bool IsSurrounded() =>
            _enemyDetector != null && _enemyDetector.IsSurrounded();

        #endregion

        #region Public Movement Methods

        public void ApplyMovement() => ApplyMovement(1f);

        /// <summary>Snap rotation toward nearest enemy or move direction.</summary>
        public void FaceNearestEnemyOrForward() {
            GameObject nearest = GetNearestEnemy();
            Vector3 targetDir;
            if (nearest != null) {
                targetDir = nearest.transform.position - transform.position;
                targetDir.y = 0f;
            } else if (Input.MoveInput.sqrMagnitude > 0.01f) {
                targetDir = new Vector3(Input.MoveInput.x, 0f, Input.MoveInput.y);
            } else {
                return;
            }
            if (targetDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(targetDir.normalized, Vector3.up);
        }

        /// <summary>Forward impulse for attack step. comboIndex is 1-based.</summary>
        public void ApplyAttackStep(int comboIndex) {
            if (_attackStepForce == null || _attackStepForce.Length == 0) return;
            int idx = Mathf.Clamp(comboIndex - 1, 0, _attackStepForce.Length - 1);
            Rb.linearVelocity = new Vector3(
                transform.forward.x * _attackStepForce[idx],
                Rb.linearVelocity.y,
                transform.forward.z * _attackStepForce[idx]);
        }
        public void ApplyMovement(float speedMultiplier) {
            Vector3 moveDir = new Vector3(Input.MoveInput.x, 0f, Input.MoveInput.y);
            float speed = _moveSpeed * speedMultiplier;

            Rb.linearVelocity = new Vector3(
                moveDir.x * speed,
                Rb.linearVelocity.y,
                moveDir.z * speed);

            // Only rotate when actually moving to prevent spinning
            if (moveDir.sqrMagnitude > 0.01f) {
                Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }
        }

        public void ApplyJump() {
            Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, _jumpForce, Rb.linearVelocity.z);
            Input.ConsumeJumpInput();
            AddChaos(20f, ChaosSource.Jump);
        }

        public void StartDashCooldown() => _dashCooldownTimer = _playerProperties.dashCooldown;

        public void SetDashInvincible(bool value) => IsDashInvincible = value;

        public void AddChaos(float amount, ChaosSource source = ChaosSource.Manual) {
            if (IsOverloaded) return;
            ChaosMeter = Mathf.Min(ChaosMeter + amount, _maxChaosThreshold);
            _chaosMeterVar?.SetValue(ChaosMeter);
            OnChaosChanged.Invoke(ChaosMeter / _maxChaosThreshold);
        }

        public void ResetChaos() {
            ChaosMeter = 0f;
            _chaosMeterVar?.SetValue(0f);
            OnChaosChanged.Invoke(0f);
        }

        public void InitiateOverload() {
            OverloadTimer = _overloadDuration;
            CooldownTimer = _overloadCooldown;
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

        /// <summary>Called by animation event to activate attack hitbox.</summary>
        public void OnHitboxActivate() => _attackHitbox?.Activate();

        /// <summary>Called by animation event to deactivate attack hitbox.</summary>
        public void OnHitboxDeactivate() => _attackHitbox?.Deactivate();

        /// <summary>Called by animation event: combo window opens, next input accepted.</summary>
        public void OnComboWindowOpen() => _attackState?.OnComboWindowOpen();

        /// <summary>Called by animation event: combo window closes.</summary>
        public void OnComboWindowClose() => _attackState?.OnComboWindowClose();

        #endregion

        #region IDamageable

        public float Health => 100f; // Player has full health system in PlayerHealth component
        public float MaxHealth => 100f;
        public System.Action<float> OnHealthChanged { get; set; } = (health) => { };
        public System.Action OnDeath { get; set; } = () => { };

        public void TakeDamage(float amount, DamageInfo info) {
            if (IsDashInvincible) return;
            AddChaos(amount * 0.5f, ChaosSource.Damage);
        }

        #endregion
    }
}