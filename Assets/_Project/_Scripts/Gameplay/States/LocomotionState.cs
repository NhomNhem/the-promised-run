using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class LocomotionState : BaseState {
        private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");

        public LocomotionState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _animator.Play(LocomotionHash);
        }

        public override void OnUpdate() {
            base.OnUpdate();

            // Use input magnitude as forward speed — character always faces move direction
            float speed = _playerController.Input.MoveInput.magnitude;
            _animator.SetFloat("VelocityZ", Mathf.Clamp01(speed));
            _animator.SetFloat("VelocityX", 0f);
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement();
        }
    }
}
