using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    /// <summary>
    /// System Overload state: player has "safety window" where Helper System is muted.
    /// Player can move freely, attack is stronger, enemies are stunned.
    /// </summary>
    public class OverloadState : BaseState {
        private static readonly int OverloadGlitchHash = Animator.StringToHash("Overload_Glitch");

        public OverloadState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _playerController.InitiateOverload();

            // Only play if state exists in controller
            if (_animator != null && _animator.HasState(0, OverloadGlitchHash))
                _animator.Play(OverloadGlitchHash);
        }

        public override void OnUpdate() {
            base.OnUpdate();
            // Player can still move during overload — this is the "safety window"
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
