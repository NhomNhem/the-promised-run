using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class OverloadState : BaseState {
        private const string OverloadAnim = "Overload_Glitch";
        private const float  BlendTime    = 0.1f;

        public OverloadState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _playerController.InitiateOverload();
            if (_animator.HasState(0, Animator.StringToHash(OverloadAnim)))
                _animator.CrossFade(OverloadAnim, BlendTime, 0);
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement();
        }

        public override void OnExit() {
            base.OnExit();
            _playerController.EndOverload();
        }
    }
}
