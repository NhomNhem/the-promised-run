using UnityEngine;
using TMPro;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Combo counter HUD — GDD §4.2:
    /// - Shows current combo hit count
    /// - Flickers (half opacity) when popup interrupts combo (suspended state)
    /// - Resets when combo ends
    /// Call SetCombo() from AttackState each hit.
    /// Call SetSuspended(true) when popup interrupts.
    /// </summary>
    public class ComboCounterUI : MonoBehaviour {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private CanvasGroup     _canvasGroup;

        [Header("Config")]
        [SerializeField] private float _flickerSpeed  = 12f;
        [SerializeField] private float _hideDelay     = 1.5f;  // hide after combo ends

        private int   _comboCount;
        private bool  _suspended;
        private float _hideTimer;
        private bool  _visible;

        private void Awake() {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void Update() {
            if (!_visible) return;

            if (_suspended) {
                // Flicker at half opacity
                float flicker = 0.4f + 0.3f * Mathf.Sin(Time.time * _flickerSpeed);
                if (_canvasGroup != null) _canvasGroup.alpha = flicker;
            } else {
                if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            }

            // Auto-hide after delay
            if (_hideTimer > 0f) {
                _hideTimer -= Time.deltaTime;
                if (_hideTimer <= 0f) SetVisible(false);
            }
        }

        /// <summary>Called by AttackState on each hit.</summary>
        public void SetCombo(int count) {
            _comboCount = count;
            _suspended  = false;
            _hideTimer  = 0f;

            if (_comboText != null)
                _comboText.text = count > 1 ? $"×{count}" : "";

            SetVisible(count > 0);
        }

        /// <summary>Called when combo ends — start hide countdown.</summary>
        public void EndCombo() {
            _suspended = false;
            _hideTimer = _hideDelay;
        }

        /// <summary>Called when popup interrupts combo (suspended, not reset).</summary>
        public void SetSuspended(bool suspended) {
            _suspended = suspended;
        }

        private void SetVisible(bool visible) {
            _visible = visible;
            if (_canvasGroup != null) _canvasGroup.alpha = visible ? 1f : 0f;
            gameObject.SetActive(visible);
        }
    }
}
