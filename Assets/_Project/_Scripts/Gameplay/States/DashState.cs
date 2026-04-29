using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    /// <summary>
    /// Dash state: 3 units distance, I-frame first 6 frames, 1.2s cooldown.
    /// Dash direction = move input or forward if no input.
    /// </summary>
    public class DashState : BaseState {
        private const string DashAnim    = "Dash";
        private const float  BlendTime   = 0.05f;
        private const float  DashTime    = 0.18f;   // duration of dash movement
        private const float  IFrameTime  = 6f / 60f; // 6 frames invincibility
        private const float  DashSpeed   = 16f;      // 3 units / 0.18s ≈ 16.7 u/s

        private float _dashTimer;
        private Vector3 _dashDir;

        public bool IsDashing => _dashTimer > 0f;
        public bool IsInvincible => _dashTimer > (DashTime - IFrameTime);

        public DashState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _dashTimer = DashTime;

            // Direction: move input or forward
            Vector2 input = _playerController.Input.MoveInput;
            _dashDir = input.sqrMagnitude > 0.01f
                ? new Vector3(input.x, 0f, input.y).normalized
                : _playerController.transform.forward;

            // Face dash direction
            if (_dashDir.sqrMagnitude > 0.01f)
                _playerController.transform.rotation =
                    Quaternion.LookRotation(_dashDir, Vector3.up);

            _playerController.Input.ConsumeDashInput();
            _playerController.StartDashCooldown();

            // Play animation if exists, otherwise skip gracefully
            if (_animator.HasState(0, Animator.StringToHash(DashAnim)))
                _animator.CrossFade(DashAnim, BlendTime, 0);
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            if (_dashTimer <= 0f) return;

            // Apply dash velocity (override Y to keep gravity)
            _playerController.Rb.linearVelocity = new Vector3(
                _dashDir.x * DashSpeed,
                _playerController.Rb.linearVelocity.y,
                _dashDir.z * DashSpeed);
        }

        public override void OnUpdate() {
            base.OnUpdate();
            _dashTimer -= Time.deltaTime;
        }

        public override void OnExit() {
            base.OnExit();
            // Bleed off dash velocity
            _playerController.Rb.linearVelocity = new Vector3(
                _playerController.Rb.linearVelocity.x * 0.3f,
                _playerController.Rb.linearVelocity.y,
                _playerController.Rb.linearVelocity.z * 0.3f);
        }

        /// <summary>True when dash movement is complete.</summary>
        public bool CanExit => _dashTimer <= 0f;
    }
}
