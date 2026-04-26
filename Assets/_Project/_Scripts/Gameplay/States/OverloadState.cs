using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class OverloadState : BaseState {
        private static readonly int OverloadHash = Animator.StringToHash("Overload");

        public OverloadState(PlayerController playerController, Animator animator) 
            : base(playerController, animator) {
        }

        public override void OnEnter() {
            base.OnEnter();
            
            _playerController.InitiateOverload();
            
            _animator.Play(OverloadHash); 
        }

        public override void OnUpdate() {
            base.OnUpdate();
            
            // _playerController.ApplyMovement();
        }

        public override void OnExit() {
            base.OnExit();
            // Xử lý logic khi hết thời gian an toàn (như bật lại UI spam)
        }
    }
}