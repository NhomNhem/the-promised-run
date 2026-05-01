using UnityEngine;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Audio;

namespace ThePromisedRun.Gameplay.States {
    /// <summary>
    /// Dash state: 3 units distance, I-frame first 6 frames, 1.2s cooldown.
    /// Dash direction = move input or forward if no input.
    /// All timing/speed params read from PlayerController (backed by PlayerProperties SO).
    /// </summary>
    public class DashState : BaseState {
        private const string DashAnim  = "Dash";
        private const float  BlendTime = 0.05f;

        // --- 4.1: backing-field timers and direction ---
        private float   _dashTimer;
        private float   _iFrameTimer;
        private Vector3 _dashDir;

        // --- 4.1: backing-field properties (testable without Time.deltaTime) ---
        public bool CanExit      { get; private set; }
        public bool IsInvincible { get; private set; }

        public DashState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        // --- 4.2: OnEnter with I-frame, chaos, juice, and SO params ---
        public override void OnEnter() {
            CanExit      = false;
            IsInvincible = false;

            // Read params from SO via PlayerController
            _dashTimer   = _playerController.DashDuration;
            _iFrameTimer = _playerController.DashIFrameDuration;
            IsInvincible = true;

            // Determine dash direction
            Vector2 moveInput = _playerController.Input.MoveInput;
            if (moveInput.sqrMagnitude > 0.01f)
                _dashDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            else
                _dashDir = _playerController.transform.forward;

            // Consume input and start cooldown
            _playerController.Input.ConsumeDashInput();
            _playerController.StartDashCooldown();

            // Mark air dash used if not grounded (prevents spam)
            if (!_playerController.IsGrounded)
                _playerController.ConsumeAirDash();

            // Chaos contribution
            _playerController.AddChaos(_playerController.ChaosPerDash, ChaosSource.Dash);

            // Juice
            _playerController.Juice?.OnDash();
            AudioManager.Instance?.PlayDash();

            // Animation (with null/missing clip guard)
            if (_animator != null) {
                int dashHash = Animator.StringToHash(DashAnim);
                if (_animator.HasState(0, dashHash))
                    _animator.CrossFade(DashAnim, BlendTime, 0);
                else
                    Debug.LogWarning("[DashState] 'Dash' animation clip not found. Skipping CrossFade.");
            }
        }

        // --- 4.3: OnUpdate with separate timer countdown ---
        public override void OnUpdate() {
            _dashTimer   -= Time.deltaTime;
            _iFrameTimer -= Time.deltaTime;

            if (_iFrameTimer <= 0f && IsInvincible)
                IsInvincible = false;

            if (_dashTimer <= 0f)
                CanExit = true;
        }

        // --- 4.4: OnFixedUpdate using DashSpeed from PlayerController ---
        public override void OnFixedUpdate() {
            if (CanExit) return;
            _playerController.Rb.linearVelocity = new Vector3(
                _dashDir.x * _playerController.DashSpeed,
                _playerController.Rb.linearVelocity.y,
                _dashDir.z * _playerController.DashSpeed);
        }

        // --- 4.5: OnExit with invincibility safety clear ---
        public override void OnExit() {
            IsInvincible = false; // safety guard for early exit
            Vector3 vel = _playerController.Rb.linearVelocity;
            _playerController.Rb.linearVelocity = new Vector3(
                vel.x * 0.3f,
                vel.y,
                vel.z * 0.3f);
        }
    }
}
