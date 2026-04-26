using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class LocomotionState : BaseState {
        private static readonly int LocomotionHash = Animator.StringToHash("Locomotion"); 

        public LocomotionState(PlayerController playerController, Animator animator) : base(playerController, animator) {
        }

        public override void OnEnter() {
            base.OnEnter();
            _animator.Play(LocomotionHash);
        }

        public override void OnUpdate() {
            base.OnUpdate();
            
            float velX = _playerController.Rb.linearVelocity.x;
            float velZ = _playerController.Rb.linearVelocity.z;
            _animator.SetFloat("VelocityX", Mathf.Clamp(velX / _playerController.MoveSpeed, -1, 1));
            _animator.SetFloat("VelocityZ", Mathf.Clamp(velZ / _playerController.MoveSpeed, -1, 1));
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement(); 
        }
    }
}