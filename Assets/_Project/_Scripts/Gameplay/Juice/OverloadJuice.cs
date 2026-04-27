using UnityEngine;

namespace ThePromisedRun.Gameplay.Juice {
    /// <summary>
    /// Visual feedback during System Overload:
    /// - Pulses the visual scale to signal "powered up" state
    /// - Can be extended with post-processing (chromatic aberration, bloom)
    /// </summary>
    public class OverloadJuice : MonoBehaviour, IJuice {
        [Header("References")]
        [SerializeField] private Transform visual;

        [Header("Pulse Settings")]
        [SerializeField] private float pulseSpeed     = 6f;
        [SerializeField] private float pulseAmplitude = 0.04f;

        [Header("Overload Tint")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color overloadColor  = new Color(1f, 0.4f, 0f); // orange
        [SerializeField] private Color normalColor    = Color.white;

        private bool    _active;
        private float   _pulseTimer;
        private Vector3 _baseScale;

        private void Awake() {
            if (visual == null) visual = transform.Find("Visual");
            _baseScale = visual != null ? visual.localScale : Vector3.one;
        }

        private void Update() {
            if (!_active || visual == null) return;

            _pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(_pulseTimer) * pulseAmplitude;
            visual.localScale = _baseScale * pulse;
        }

        /// <summary>Start overload visual effects.</summary>
        public void Play() => StartOverload();

        public void StartOverload() {
            _active = true;
            _pulseTimer = 0f;
            SetTint(overloadColor);
        }

        public void StopOverload() {
            _active = false;
            if (visual != null) visual.localScale = _baseScale;
            SetTint(normalColor);
        }

        private void SetTint(Color color) {
            foreach (var r in renderers) {
                if (r != null)
                    r.material.color = color;
            }
        }
    }
}
