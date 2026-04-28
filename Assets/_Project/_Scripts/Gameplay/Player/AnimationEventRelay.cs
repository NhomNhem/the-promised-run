using UnityEngine;
using ThePromisedRun.Gameplay;

namespace ThePromisedRun.Gameplay.Player {
    /// <summary>
    /// Sits on the same GameObject as the Animator (Knight).
    /// Forwards animation events up to PlayerController on the root.
    /// </summary>
    public class AnimationEventRelay : MonoBehaviour {
        private PlayerController _playerController;

        private void Awake() {
            _playerController = GetComponentInParent<PlayerController>(true);
            if (_playerController == null)
                Debug.LogWarning("[AnimationEventRelay] PlayerController not found in parent hierarchy.");
        }

        public void OnHitboxActivate()   => _playerController?.OnHitboxActivate();
        public void OnHitboxDeactivate() => _playerController?.OnHitboxDeactivate();
        public void OnComboWindowOpen()  => _playerController?.OnComboWindowOpen();
        public void OnComboWindowClose() => _playerController?.OnComboWindowClose();
    }
}
