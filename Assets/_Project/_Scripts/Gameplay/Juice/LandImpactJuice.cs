using UnityEngine;

namespace ThePromisedRun.Gameplay.Juice {
    /// <summary>
    /// Landing impact juice: camera punch + optional screen shake.
    /// Attach to the Player GameObject.
    /// </summary>
    public class LandImpactJuice : MonoBehaviour, IJuice {
        [Header("Camera Punch")]
        [SerializeField] private float punchStrength = 0.15f;
        [SerializeField] private float punchDuration = 0.18f;
        [SerializeField] private float punchDecay    = 8f;

        private Transform _camTransform;
        private Vector3   _camOriginalLocalPos;
        private float     _punchTimer;
        private bool      _punching;

        private void Awake() {
            var mainCam = Camera.main;
            if (mainCam != null) {
                _camTransform = mainCam.transform;
                _camOriginalLocalPos = _camTransform.localPosition;
            }
        }

        private void LateUpdate() {
            if (!_punching || _camTransform == null) return;

            _punchTimer += Time.deltaTime;
            float t = _punchTimer / punchDuration;

            // Punch down then recover
            float offset = punchStrength * Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-punchDecay * t);
            _camTransform.localPosition = _camOriginalLocalPos + Vector3.down * offset;

            if (_punchTimer >= punchDuration) {
                _camTransform.localPosition = _camOriginalLocalPos;
                _punching = false;
            }
        }

        public void Play() {
            if (_camTransform == null) return;
            _punchTimer = 0f;
            _punching = true;
        }
    }
}
