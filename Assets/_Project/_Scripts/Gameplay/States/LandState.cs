using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    /// <summary>
    /// Handles the landing phase: plays Jump_Land animation, fires juice,
    /// briefly locks movement, then transitions to Locomotion.
    /// </summary>
    public class LandState : BaseState {
        private static readonly int JumpLandHash = Animator.StringToHash("Jump_Land");

        private float _landDuration = 0.2f;

        private float _landTimer;

        public LandState(PlayerController playerController, Animator animator, float landDuration = 0.2f)
            : base(playerController, animator) {
            _landDuration = landDuration;
        }

        public override void OnEnter() {
            base.OnEnter();
            _landTimer = 0f;

            // Stop horizontal velocity — landing plants the character
            _playerController.Rb.linearVelocity = new Vector3(
                0f,
                _playerController.Rb.linearVelocity.y,
                0f
            );

            _animator.Play(JumpLandHash);
            _playerController.Juice?.OnLand();
        }

        public override void OnUpdate() {
            base.OnUpdate();
            _landTimer += Time.deltaTime;
        }

        /// <summary>True when the landing animation window has elapsed.</summary>
        public bool IsLandingComplete => _landTimer >= _landDuration;
    }
}
