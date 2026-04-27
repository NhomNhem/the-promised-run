using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class JumpState : BaseState {
        private static readonly int JumpStartHash = Animator.StringToHash("Jump_Start");
        private static readonly int JumpAirHash   = Animator.StringToHash("Jump_Air");

        private const float MinAirTime = 0.15f;

        private float _airTimer;
        private bool  _isAscending;

        public JumpState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _airTimer    = 0f;
            _isAscending = false;

            _animator.Play(JumpStartHash);
            _playerController.ApplyJump();
            _playerController.Juice?.OnTakeoff();
        }

        public override void OnUpdate() {
            base.OnUpdate();
            _airTimer += Time.deltaTime;

            if (_airTimer < MinAirTime) return;

            // Ascending → play air animation once
            if (_playerController.Rb.linearVelocity.y > 0f && !_isAscending) {
                _animator.Play(JumpAirHash);
                _isAscending = true;
            }
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement();
        }

        /// <summary>
        /// Ready to transition to LandState: airborne long enough AND touching ground
        /// while falling.
        /// </summary>
        public bool CanLand =>
            _airTimer >= MinAirTime &&
            _playerController.IsGrounded &&
            _playerController.Rb.linearVelocity.y <= 0f;
    }
}
