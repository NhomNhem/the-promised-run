using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class LocomotionState : BaseState {
        private const string StateName  = "Locomotion";
        private const float  BlendTime  = 0.15f;

        public LocomotionState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            // Set params from actual input immediately — no stale values
            Vector2 input = _playerController.Input != null
                ? _playerController.Input.MoveInput
                : Vector2.zero;
            _animator.SetFloat("VelocityX", input.x);
            _animator.SetFloat("VelocityZ", input.magnitude);
            _animator.CrossFade(StateName, BlendTime, 0);
        }

        public override void OnUpdate() {
            base.OnUpdate();
            Vector2 input = _playerController.Input != null
                ? _playerController.Input.MoveInput
                : Vector2.zero;
            _animator.SetFloat("VelocityX", input.x);
            _animator.SetFloat("VelocityZ", input.magnitude);
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement();
        }
    }
}
