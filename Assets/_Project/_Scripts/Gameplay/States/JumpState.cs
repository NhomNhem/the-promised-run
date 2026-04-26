using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class JumpState : BaseState {
        private static readonly int JumpStartHash = Animator.StringToHash("Jump_Start");
        private static readonly int JumpAirHash = Animator.StringToHash("Jump_Air");
        private static readonly int JumpLandHash = Animator.StringToHash("Jump_Land");
        private bool _hasStartedJump;

        public JumpState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _animator.Play(JumpStartHash);
            _animator.SetTrigger("JumpTrigger");
            _playerController.ApplyJump();
            _hasStartedJump = false;
        }

        public override void OnUpdate() {
            base.OnUpdate();
            _playerController.ApplyMovement();
            
            if (!_playerController.IsGrounded) {
                if (_playerController.Rb.linearVelocity.y > 0 && !_hasStartedJump) {
                    _animator.Play(JumpAirHash);
                    _hasStartedJump = true;
                }
                else if (_playerController.Rb.linearVelocity.y <= 0) {
                    _animator.Play(JumpLandHash);
                }
            }
        }
    }
}