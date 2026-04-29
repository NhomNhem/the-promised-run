using UnityEngine;
using UnityEngine.UIElements;
using OpenUtility.Data;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Game HUD — binds health-bar and chaos-bar to ScriptableFloat events.
    /// Receives its VisualElement root from HUDManager (shared UIDocument).
    /// </summary>
    public class GameHUDController : MonoBehaviour {
        [Header("ScriptableVariables")]
        [SerializeField] private ScriptableFloat _healthVar;
        [SerializeField] private ScriptableFloat _chaosMeterVar;

        [Header("Config")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxChaos  = 100f;

        private VisualElement _healthFill;
        private VisualElement _chaosFill;
        private VisualElement _overloadBanner;
        private bool          _initialized;

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[GameHUDController] Initialize called with null root.");
                return;
            }

            _healthFill     = root.Q<VisualElement>("health-bar-fill");
            _chaosFill      = root.Q<VisualElement>("chaos-bar-fill");
            _overloadBanner = root.Q<VisualElement>("overload-banner");
            _initialized    = true;

            BindEvents();
        }

        private void OnEnable() {
            // Re-bind if already initialized (e.g. GameObject re-enabled)
            if (_initialized) BindEvents();
        }

        private void OnDisable() {
            _healthVar?.ValueChanged.RemoveListener(OnHealthChanged);
            _chaosMeterVar?.ValueChanged.RemoveListener(OnChaosChanged);
        }

        private void BindEvents() {
            _healthVar?.ValueChanged.RemoveListener(OnHealthChanged);
            _chaosMeterVar?.ValueChanged.RemoveListener(OnChaosChanged);

            if (_healthVar != null) {
                _healthVar.ValueChanged.AddListener(OnHealthChanged);
                OnHealthChanged(_healthVar.GetValue());
            }
            if (_chaosMeterVar != null) {
                _chaosMeterVar.ValueChanged.AddListener(OnChaosChanged);
                OnChaosChanged(_chaosMeterVar.GetValue());
            }
        }

        private void OnHealthChanged(float value) {
            if (_healthFill == null) return;
            float norm = Mathf.Clamp01(value / _maxHealth);
            _healthFill.style.width = new StyleLength(new Length(norm * 100f, LengthUnit.Percent));
            _healthFill.RemoveFromClassList("medium");
            _healthFill.RemoveFromClassList("low");
            if (norm < 0.3f)      _healthFill.AddToClassList("low");
            else if (norm < 0.6f) _healthFill.AddToClassList("medium");
        }

        private void OnChaosChanged(float value) {
            if (_chaosFill == null) return;
            float norm = Mathf.Clamp01(value / _maxChaos);
            _chaosFill.style.width = new StyleLength(new Length(norm * 100f, LengthUnit.Percent));
            _chaosFill.RemoveFromClassList("low");
            _chaosFill.RemoveFromClassList("medium");
            _chaosFill.RemoveFromClassList("high");
            _chaosFill.RemoveFromClassList("full");
            if (norm >= 1f)        _chaosFill.AddToClassList("full");
            else if (norm >= 0.7f) _chaosFill.AddToClassList("high");
            else if (norm >= 0.3f) _chaosFill.AddToClassList("medium");
            else                   _chaosFill.AddToClassList("low");

            if (_overloadBanner != null) {
                if (norm >= 1f) _overloadBanner.RemoveFromClassList("hidden");
                else            _overloadBanner.AddToClassList("hidden");
            }
        }
    }
}
