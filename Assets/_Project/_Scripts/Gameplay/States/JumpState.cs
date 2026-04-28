using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class JumpState : BaseState {
        private const string JumpStart = "Jump_Start";
        private const string JumpAir   = "Jump_Air";
        private const float  BlendTime = 0.05f;
        private const float  MinAirTime = 0.15f;

        private float _airTimer;
        private bool  _isAscending;

        public JumpState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _airTimer    = 0f;
            _isAscending = false;
            _animator.CrossFade(JumpStart, BlendTime, 0);
            _playerController.ApplyJump();
            _playerController.Juice?.OnTakeoff();
        }

        public override void OnUpdate() {
            base.OnUpdate();
            _airTimer += Time.deltaTime;
            if (_airTimer < MinAirTime) return;

            if (_playerController.Rb.linearVelocity.y > 0f && !_isAscending) {
                _animator.CrossFade(JumpAir, BlendTime, 0);
                _isAscending = true;
            }
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement();
        }

        public bool CanLand =>
            _airTimer >= MinAirTime &&
            _playerController.IsGrounded &&
            _playerController.Rb.linearVelocity.y <= 0f;
    }
}
