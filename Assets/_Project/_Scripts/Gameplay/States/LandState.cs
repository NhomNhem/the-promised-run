using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public class LandState : BaseState {
        private const string LandAnim  = "Jump_Land";
        private const float  BlendTime = 0.05f;

        private readonly float _landDuration;
        private float _landTimer;

        public LandState(PlayerController playerController, Animator animator, float landDuration = 0.25f)
            : base(playerController, animator) {
            _landDuration = landDuration;
        }

        public override void OnEnter() {
            base.OnEnter();
            _landTimer = 0f;
            _playerController.Rb.linearVelocity = new Vector3(
                0f, _playerController.Rb.linearVelocity.y, 0f);
            _animator.SetFloat("VelocityX", 0f);
            _animator.SetFloat("VelocityZ", 0f);
            _animator.CrossFade(LandAnim, BlendTime, 0);
            _playerController.Juice?.OnLand();
        }

        public override void OnUpdate() {
            base.OnUpdate();
            _landTimer += Time.deltaTime;
        }

        public bool IsLandingComplete => _landTimer >= _landDuration;
    }
}
