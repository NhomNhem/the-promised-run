using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class JumpState : BaseState {
        private static readonly int JumpStartHash = Animator.StringToHash("Jump_Start");
        private static readonly int JumpAirHash = Animator.StringToHash("Jump_Air");
        private static readonly int JumpLandHash = Animator.StringToHash("Jump_Land");

        private bool _hasStartedJump;
        private bool _isAscending;

        public JumpState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _hasStartedJump = false;
            _isAscending = false;

            // Trigger jump immediately on state enter — the FSM transition already
            // confirmed IsJumpPressed && IsGrounded, so no buffer needed here.
            _animator.Play(JumpStartHash);
            _playerController.ApplyJump();
            _hasStartedJump = true;
        }

        public override void OnUpdate() {
            base.OnUpdate();

            if (!_hasStartedJump) return;

            float vy = _playerController.Rb.linearVelocity.y;

            if (!_playerController.IsGrounded) {
                if (vy > 0f) {
                    // Ascending — play air animation once
                    if (!_isAscending) {
                        _animator.Play(JumpAirHash);
                        _isAscending = true;
                    }
                } else {
                    // Descending / about to land
                    _animator.Play(JumpLandHash);
                }
            }
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            // Allow horizontal movement while airborne
            _playerController.ApplyMovement();
        }
    }
}
