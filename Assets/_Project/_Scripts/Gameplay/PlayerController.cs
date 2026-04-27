using UnityEngine;
using System.Linq;
using RaycastPro.RaySensors;
using ThePromisedRun.Core.FSM;
using ThePromisedRun.Gameplay.States;
using ThePromisedRun.Gameplay.Input;
using UnityEngine.Events;

namespace ThePromisedRun.Gameplay {
    public class PlayerController : MonoBehaviour {
        #region Inspector Fields
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;

        [Header("System Overload Settings")]
        [SerializeField] private float overloadDuration = 3f;
        [SerializeField] private float overloadCooldown = 5f;
        [SerializeField] private float maxChaosThreshold = 100f;
        [SerializeField] private float chaosDecayRate = 10f;

        [Header("References")]
        [SerializeField] private Transform visual;
        [SerializeField] private Transform detector;
        [SerializeField] private BasicRay groundRay;
        #endregion

        #region Public Properties & State
        public Rigidbody Rb { get; private set; }
        public Animator Anim { get; private set; }
        public InputReader Input { get; private set; }
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public bool IsGrounded { get; private set; }
        public float OverloadTimer { get; private set; }
        public float CooldownTimer { get; private set; }
        public float ChaosMeter { get; private set; }
        public bool IsOverloaded => OverloadTimer > 0f;
        #endregion

        #region Events
        // Event for chaos/overload state changes (UI/camera can subscribe)
        public UnityEvent<float> OnChaosChanged = new UnityEvent<float>();
        public UnityEvent OnOverloadStarted = new UnityEvent();
        public UnityEvent OnOverloadEnded = new UnityEvent();
        #endregion

        #region Private Fields
        private StateMachine _stateMachine;
        private int _groundContacts;
        #endregion

        private void Awake() {
            // Get required components
            Rb = GetComponent<Rigidbody>();
            Input = GetComponent<InputReader>();

            // Animator assignment
            if (visual != null)
                Anim = visual.GetComponent<Animator>();
            else
                Anim = GetComponentInChildren<Animator>();

            // Detector/groundRay assignment
            if (detector == null) {
                detector = Enumerable.Range(0, transform.childCount)
                    .Select(i => transform.GetChild(i))
                    .FirstOrDefault(c => c.name == "Detector");
            }
            if (detector != null) {
                groundRay = detector.GetComponent<BasicRay>();
            }

            // FSM setup
            _stateMachine = new StateMachine();
            SetupStateMachine();

            // Camera follow/target setup placeholder (Cinemachine integration)
            // TODO: Assign Cinemachine virtual camera follow/target here if needed
        }

        private void SetupStateMachine() {
            // State instances
            var locomotion = new LocomotionState(this, Anim);
            var jump = new JumpState(this, Anim);
            var overload = new OverloadState(this, Anim);

            // Transitions
            _stateMachine.AddTransition(locomotion, jump, new FuncPredicate(() => Input.IsJumpPressed && IsGrounded && !IsOverloaded));
            _stateMachine.AddTransition(jump, locomotion, new FuncPredicate(() => IsGrounded && Rb.linearVelocity.y <= 0 && !IsOverloaded));
            _stateMachine.AddAnyTransition(overload, new FuncPredicate(() => ChaosMeter >= maxChaosThreshold && CooldownTimer <= 0));
            _stateMachine.AddTransition(overload, locomotion, new FuncPredicate(() => OverloadTimer <= 0 && IsGrounded));

            _stateMachine.SetState(locomotion);
        }

        private void Update() {
            _stateMachine.Update();
            HandleTimers();
            CheckGround();
        }

        private void FixedUpdate() {
            _stateMachine.FixedUpdate();
        }

        private void HandleTimers() {
            if (OverloadTimer > 0) OverloadTimer -= Time.deltaTime;
            if (CooldownTimer > 0) CooldownTimer -= Time.deltaTime;

            if (OverloadTimer <= 0 && ChaosMeter > 0) {
                ChaosMeter = Mathf.Max(0, ChaosMeter - chaosDecayRate * Time.deltaTime);
                OnChaosChanged.Invoke(ChaosMeter);
            }
        }

        private void OnEnable() {
            if (detector != null) {
                var detectorCollider = detector.GetComponent<Collider>();
                if (detectorCollider != null) detectorCollider.enabled = true;
            }
        }

        private void OnDisable() {
            if (detector != null) {
                var detectorCollider = detector.GetComponent<Collider>();
                if (detectorCollider != null) detectorCollider.enabled = false;
            }
        }

        private void CheckGround() {
            if (groundRay != null) {
                IsGrounded = groundRay.Performed;
                // Nếu cần, có thể đọc groundRay.HitInfo để lấy thông tin va chạm
            } else {
                IsGrounded = _groundContacts > 0;
            }
        }

        #region Actions (Called by States or external factors)

        /// <summary>
        /// Applies movement input to the Rigidbody.
        /// Visual rotation is NOT modified here — the character always faces the camera (+Z).
        /// Strafe direction is handled entirely by VelocityX in the BlendTree.
        /// </summary>
        public void ApplyMovement() {
            Rb.linearVelocity = new Vector3(Input.MoveInput.x * moveSpeed, Rb.linearVelocity.y, Input.MoveInput.y * moveSpeed);
        }

        /// <summary>
        /// Applies jump force and consumes jump input.
        /// </summary>
        public void ApplyJump() {
            Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, jumpForce, 0);
            Input.ConsumeJumpInput();
            AddChaos(20f);
        }

        /// <summary>
        /// Adds chaos to the meter and invokes event for UI/camera.
        /// </summary>
        public void AddChaos(float amount) {
            if (OverloadTimer > 0) return;
            ChaosMeter += amount;
            OnChaosChanged.Invoke(ChaosMeter);
            // TODO: Fire event to update Chaos UI bar here
        }

        /// <summary>
        /// Initiates overload state, resets chaos, and invokes events.
        /// </summary>
        public void InitiateOverload() {
            OverloadTimer = overloadDuration;
            CooldownTimer = overloadCooldown;
            ChaosMeter = 0f;

            Debug.Log("System Muted! Safety window active.");
            OnOverloadStarted.Invoke();
            // TODO: Fire event to notify SystemAdvisorManager to hide UI clutter
        }

        /// <summary>
        /// Call this when overload ends (for event-driven camera/UI logic).
        /// </summary>
        public void EndOverload() {
            OnOverloadEnded.Invoke();
        }

        #endregion
    }
}