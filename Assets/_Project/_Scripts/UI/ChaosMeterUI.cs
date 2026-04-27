using UnityEngine;
using UnityEngine.UIElements;
using ThePromisedRun.Gameplay;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Drives the Chaos Meter bar and Overload banner in the Game HUD.
    /// Subscribes to PlayerController events — no polling.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ChaosMeterUI : MonoBehaviour {
        [SerializeField] private PlayerController _player;

        private VisualElement _fill;
        private VisualElement _overloadBanner;

        private void Awake() {
            if (_player == null)
                _player = FindFirstObjectByType<PlayerController>();
        }

        private void OnEnable() {
            var doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;

            _fill          = root.Q<VisualElement>("chaos-bar-fill");
            _overloadBanner = root.Q<VisualElement>("overload-banner");

            if (_player == null) return;
            _player.OnChaosChanged.AddListener(OnChaosChanged);
            _player.OnOverloadStarted.AddListener(OnOverloadStarted);
            _player.OnOverloadEnded.AddListener(OnOverloadEnded);
        }

        private void OnDisable() {
            if (_player == null) return;
            _player.OnChaosChanged.RemoveListener(OnChaosChanged);
            _player.OnOverloadStarted.RemoveListener(OnOverloadStarted);
            _player.OnOverloadEnded.RemoveListener(OnOverloadEnded);
        }

        // normalized 0-1
        private void OnChaosChanged(float normalized) {
            if (_fill == null) return;

            _fill.style.width = Length.Percent(normalized * 100f);

            // Update color class based on fill level
            _fill.RemoveFromClassList("low");
            _fill.RemoveFromClassList("medium");
            _fill.RemoveFromClassList("high");
            _fill.RemoveFromClassList("full");

            string cls = normalized switch {
                >= 1f    => "full",
                >= 0.66f => "high",
                >= 0.33f => "medium",
                _        => "low"
            };
            _fill.AddToClassList(cls);
        }

        private void OnOverloadStarted() {
            _overloadBanner?.RemoveFromClassList("hidden");
        }

        private void OnOverloadEnded() {
            _overloadBanner?.AddToClassList("hidden");
            // Reset bar to empty
            if (_fill != null) _fill.style.width = Length.Percent(0f);
        }
    }
}
