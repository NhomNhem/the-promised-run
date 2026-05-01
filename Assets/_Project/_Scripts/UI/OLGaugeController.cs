using UnityEngine;
using UnityEngine.UIElements;

namespace ThePromisedRun.UI {
    /// <summary>
    /// OLGaugeController — drives the OL gauge bar in HUD.uxml.
    /// Receives VisualElement root from HUDManager (shared UIDocument).
    /// Subscribes to OverloadSystem events for state changes.
    /// </summary>
    public class OLGaugeController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Gameplay.OverloadSystem _overloadSystem;

        [Header("Config")]
        private VisualElement _gaugeBar;
        private VisualElement _gaugeContainer;
        private bool          _initialized;

        private static readonly Color ColorSafe     = new Color(0.7f, 0.7f, 0.7f); // Gray
        private static readonly Color ColorBuilding = new Color(1f,   0.8f, 0f);   // Amber
        private static readonly Color ColorCritical = new Color(1f,   0.2f, 0.2f); // Red
        private static readonly Color ColorOverload = new Color(1f,   1f,   1f);   // White

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[OLGaugeController] Initialize called with null root.");
                return;
            }

            _gaugeContainer = root.Q<VisualElement>("ol-gauge-container");
            _gaugeBar       = root.Q<VisualElement>("ol-gauge-bar");

            if (_gaugeContainer == null || _gaugeBar == null) {
                Debug.LogWarning("[OLGaugeController] 'ol-gauge-container' or 'ol-gauge-bar' not found in HUD.uxml.");
                return;
            }

            if (_overloadSystem == null)
                _overloadSystem = FindFirstObjectByType<Gameplay.OverloadSystem>();

            if (_overloadSystem != null) {
                _overloadSystem.OnOverloadTriggered += OnOverloadStart;
                _overloadSystem.OnOverloadEnded     += OnOverloadEnd;
            } else {
                Debug.LogWarning("[OLGaugeController] OverloadSystem not found.");
            }

            _initialized = true;
        }

        private void OnDestroy() {
            if (_overloadSystem != null) {
                _overloadSystem.OnOverloadTriggered -= OnOverloadStart;
                _overloadSystem.OnOverloadEnded     -= OnOverloadEnd;
            }
        }

        private void Update() {
            if (!_initialized || _overloadSystem == null || _gaugeBar == null) return;

            // Update width
            float norm = _overloadSystem.Gauge / 100f;
            _gaugeBar.style.width = new StyleLength(new Length(norm * 100f, LengthUnit.Percent));

            // Update color
            Color target;
            if (_overloadSystem.IsActive)          target = ColorOverload;
            else if (_overloadSystem.Gauge >= 70f) target = ColorCritical;
            else if (_overloadSystem.Gauge >= 30f) target = ColorBuilding;
            else                                   target = ColorSafe;

            _gaugeBar.style.backgroundColor = new StyleColor(target);
        }

        private void OnOverloadStart() {
            _gaugeContainer?.AddToClassList("overload-active");
            if (_gaugeBar != null)
                _gaugeBar.style.backgroundColor = new StyleColor(ColorOverload);
        }

        private void OnOverloadEnd() {
            _gaugeContainer?.RemoveFromClassList("overload-active");
        }
    }
}
