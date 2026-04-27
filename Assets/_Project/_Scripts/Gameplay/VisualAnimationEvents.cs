using UnityEngine;

namespace ThePromisedRun.Gameplay {
    /// <summary>
    /// Handles AnimationEvents on the visual GameObject and forwards them to PlayerController
    /// </summary>
    public class VisualAnimationEvents : MonoBehaviour {
        private PlayerController _playerController;

        private void Awake() {
            // Find the PlayerController on the parent/root GameObject
            _playerController = GetComponentInParent<PlayerController>();
            if (_playerController == null) {
                Debug.LogError("VisualAnimationEvents: Could not find PlayerController in parent hierarchy!");
            }
        }

        /// <summary>
        /// Called by animation event to activate attack hitbox
        /// </summary>
        public void OnHitboxActivate() {
            _playerController?.OnHitboxActivate();
        }

        /// <summary>
        /// Called by animation event to deactivate attack hitbox
        /// </summary>
        public void OnHitboxDeactivate() {
            _playerController?.OnHitboxDeactivate();
        }
    }
}
