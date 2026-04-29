using UnityEngine;
using UnityEngine.UIElements;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Combo counter HUD — GDD §4.2.
    /// Receives its VisualElement root from HUDManager (shared UIDocument).
    /// Queries combo-counter and combo-text from the gameplay-layer.
    /// </summary>
    public class ComboCounterUI : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float _flickerSpeed = 12f;
        [SerializeField] private float _hideDelay    = 1.5f;

        private VisualElement _comboRoot;
        private Label         _comboLabel;

        private int   _comboCount;
        private bool  _suspended;
        private float _hideTimer;
        private bool  _visible;

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[ComboCounterUI] Initialize called with null root.");
                return;
            }

            _comboRoot  = root.Q<VisualElement>("combo-counter");
            _comboLabel = root.Q<Label>("combo-text");

            if (_comboRoot == null)
                Debug.LogWarning("[ComboCounterUI] 'combo-counter' element not found in HUD root.");

            SetVisible(false);
        }

        private void Update() {
            if (!_visible || _comboRoot == null) return;

            if (_suspended) {
                float flicker = 0.4f + 0.3f * Mathf.Sin(Time.time * _flickerSpeed);
                _comboRoot.style.opacity = new StyleFloat(flicker);
            } else {
                _comboRoot.style.opacity = new StyleFloat(1f);
            }

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

            if (_comboLabel != null)
                _comboLabel.text = count > 1 ? $"×{count}" : "";

            SetVisible(count > 1);
        }

        /// <summary>Called when combo ends — start hide countdown.</summary>
        public void EndCombo() {
            _suspended = false;
            _hideTimer = _hideDelay;
        }

        /// <summary>Called when popup interrupts combo (suspended, not reset).</summary>
        public void SetSuspended(bool suspended) => _suspended = suspended;

        private void SetVisible(bool visible) {
            _visible = visible;
            if (_comboRoot == null) return;
            if (visible) {
                _comboRoot.RemoveFromClassList("hidden");
                _comboRoot.style.opacity = new StyleFloat(1f);
            } else {
                _comboRoot.AddToClassList("hidden");
            }
        }
    }
}
