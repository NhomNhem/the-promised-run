using UnityEngine;
using UnityEngine.UIElements;
using ThePromisedRun.Gameplay;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Drives the Health Bar, Chaos Meter, and Overload banner in the Game HUD.
    /// Event-driven — no polling.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ChaosMeterUI : MonoBehaviour {
        [SerializeField] private PlayerController _player;
        [SerializeField] private PlayerHealth     _health;

        // Chaos
        private VisualElement _chaosFill;
        private VisualElement _overloadBanner;

        // Health
        private VisualElement _healthFill;

        private void Awake() {
            if (_player == null) _player = FindFirstObjectByType<PlayerController>();
            if (_health == null) _health = FindFirstObjectByType<PlayerHealth>();
        }

        private void OnEnable() {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _chaosFill      = root.Q<VisualElement>("chaos-bar-fill");
            _overloadBanner = root.Q<VisualElement>("overload-banner");
            _healthFill     = root.Q<VisualElement>("health-bar-fill");

            if (_player != null) {
                _player.OnChaosChanged.AddListener(OnChaosChanged);
                _player.OnOverloadStarted.AddListener(OnOverloadStarted);
                _player.OnOverloadEnded.AddListener(OnOverloadEnded);
            }
            if (_health != null) {
                _health.OnHealthChangedUnity.AddListener(OnHealthChanged);
            }
        }

        private void OnDisable() {
            if (_player != null) {
                _player.OnChaosChanged.RemoveListener(OnChaosChanged);
                _player.OnOverloadStarted.RemoveListener(OnOverloadStarted);
                _player.OnOverloadEnded.RemoveListener(OnOverloadEnded);
            }
            if (_health != null) {
                _health.OnHealthChangedUnity.RemoveListener(OnHealthChanged);
            }
        }

        private void OnChaosChanged(float normalized) {
            if (_chaosFill == null) return;
            _chaosFill.style.width = Length.Percent(normalized * 100f);
            _chaosFill.RemoveFromClassList("low");
            _chaosFill.RemoveFromClassList("medium");
            _chaosFill.RemoveFromClassList("high");
            _chaosFill.RemoveFromClassList("full");
            _chaosFill.AddToClassList(normalized switch {
                >= 1f    => "full",
                >= 0.66f => "high",
                >= 0.33f => "medium",
                _        => "low"
            });
        }

        private void OnHealthChanged(float normalized) {
            if (_healthFill == null) return;
            _healthFill.style.width = Length.Percent(normalized * 100f);
            _healthFill.RemoveFromClassList("medium");
            _healthFill.RemoveFromClassList("low");
            if (normalized <= 0.25f)      _healthFill.AddToClassList("low");
            else if (normalized <= 0.5f)  _healthFill.AddToClassList("medium");
        }

        private void OnOverloadStarted() => _overloadBanner?.RemoveFromClassList("hidden");
        private void OnOverloadEnded() {
            _overloadBanner?.AddToClassList("hidden");
            if (_chaosFill != null) _chaosFill.style.width = Length.Percent(0f);
        }
    }
}
