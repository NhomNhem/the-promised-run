using UnityEngine;
using ThePromisedRun.Core.FSM;
using ThePromisedRun.Gameplay.States;
using ThePromisedRun.Gameplay.Input;

namespace ThePromisedRun.Gameplay {
    public class PlayerController : MonoBehaviour {
        [Header("Movement Settings")] 
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;

        [Header("System Overload Settings")] 
        [SerializeField] private float overloadDuration = 3f;
        [SerializeField] private float overloadCooldown = 5f;
        [SerializeField] private float maxChaosThreshold = 100f; 
        [SerializeField] private float chaosDecayRate = 10f;     

        // References
        public Rigidbody2D Rb { get; private set; }
        public Animator Anim { get; private set; }
        public InputReader Input { get; private set; }

        // State Variables
        public bool IsGrounded { get; private set; }
        public float OverloadTimer { get; private set; }
        public float CooldownTimer { get; private set; }
        public float ChaosMeter { get; private set; } 

        private StateMachine _stateMachine;

        private void Awake() {
            Rb = GetComponent<Rigidbody2D>();
            Anim = GetComponent<Animator>();
            Input = GetComponent<InputReader>();

            _stateMachine = new StateMachine();
            SetupStateMachine();
        }

        private void SetupStateMachine() {
            var locomotion = new LocomotionState(this, Anim);
            var jump = new JumpState(this, Anim);
            var overload = new OverloadState(this, Anim);

            _stateMachine.AddTransition(locomotion, jump, new FuncPredicate(() => Input.IsJumpPressed && IsGrounded));
            _stateMachine.AddTransition(jump, locomotion, new FuncPredicate(() => IsGrounded && Rb.linearVelocity.y <= 0));

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
            }
        }

        private void CheckGround() => IsGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground"));

        #region Actions (Được gọi bởi các State hoặc yếu tố ngoại cảnh)

        public void ApplyMovement() {
            Rb.linearVelocity = new Vector2(Input.MoveInput.x * moveSpeed, Rb.linearVelocity.y);

            if (Input.MoveInput.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(Input.MoveInput.x), 1, 1);
        }

        public void ApplyJump() {
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, jumpForce);
            Input.ConsumeJumpInput();
            AddChaos(20f); 
        }

        public void AddChaos(float amount) {
            if (OverloadTimer > 0) return; 
            ChaosMeter += amount;
            // TODO: Bắn Event cập nhật UI thanh Chaos ở đây
        }

        public void InitiateOverload() {
            OverloadTimer = overloadDuration;
            CooldownTimer = overloadCooldown;
            ChaosMeter = 0f; 

            Debug.Log("System Muted! Safety window active.");
            // TODO: Bắn Event báo cho SystemAdvisorManager tắt các UI rác
        }

        #endregion
    }
}