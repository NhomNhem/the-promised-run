using UnityEngine;
using UnityEngine.UIElements;
using OpenUtility.Data;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Game HUD — UI Toolkit, lives on the Player prefab.
    /// Binds health-bar-fill and chaos-bar-fill to ScriptableFloat events.
    /// Loaded additively via Scene_HUD or embedded in Player prefab UIDocument.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
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

        private void OnEnable() {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null) return;

            _healthFill     = root.Q<VisualElement>("health-bar-fill");
            _chaosFill      = root.Q<VisualElement>("chaos-bar-fill");
            _overloadBanner = root.Q<VisualElement>("overload-banner");

            if (_healthVar != null) {
                _healthVar.ValueChanged.AddListener(OnHealthChanged);
                OnHealthChanged(_healthVar.GetValue());
            }
            if (_chaosMeterVar != null) {
                _chaosMeterVar.ValueChanged.AddListener(OnChaosChanged);
                OnChaosChanged(_chaosMeterVar.GetValue());
            }
        }

        private void OnDisable() {
            _healthVar?.ValueChanged.RemoveListener(OnHealthChanged);
            _chaosMeterVar?.ValueChanged.RemoveListener(OnChaosChanged);
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
