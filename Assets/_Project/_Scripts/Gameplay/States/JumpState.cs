using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class JumpState : BaseState {
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        public JumpState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _animator.Play(JumpHash);

            _playerController.ApplyJump();
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement();
        }
    }
}