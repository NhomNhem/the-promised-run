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

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement(); 
        }
    }
}