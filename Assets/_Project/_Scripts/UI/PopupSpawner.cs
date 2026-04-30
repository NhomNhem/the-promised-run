using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

namespace ThePromisedRun.UI {
    /// <summary>
    /// PopupSpawner — drives popup windows in HUD.uxml.
    /// Receives VisualElement root from HUDManager (shared UIDocument).
    ///
    /// Replaces the ScriptableVariable-based PopupUI for direct OverloadSystem integration.
    /// Angry mode: after Overload ends, spawn rate ×1.5 for 10s.
    /// Gauge accumulation: +5/sec while popup is visible.
    /// </summary>
    public class PopupSpawner : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float _spawnRate        = 0.33f; // popups per second (1 every 3s)
        [SerializeField] private float _angryMultiplier  = 1.5f;
        [SerializeField] private float _angryDuration    = 10f;
        [SerializeField] private float _displayDuration  = 4f;
        [SerializeField] private float _gaugePerSecond   = 5f;    // OL gauge per second while visible

        [Header("References")]
        [SerializeField] private Gameplay.OverloadSystem _overloadSystem;

        private VisualElement _popupWindow;
        private Label         _messageLabel;
        private VisualElement _popupLayer;

        private float _spawnTimer;
        private float _currentSpawnRate;
        private float _angryTimer;
        private bool  _initialized;

        private static readonly string[] PopupMessages = {
            "JUMP!",
            "RUN!",
            "LEVEL UP! +5 WIS",
            "WARNING: Danger detected (3 seconds ago)",
            "NEW QUEST: Collect 3 Phantom Coins",
            "Auto-logout in 5s...",
            "RECOMMENDED: Use Healing Potion!",
            "ACHIEVEMENT UNLOCKED: Breathing",
            "SYSTEM RECALIBRATING...",
            "TIP: Try not to die",
            "REMINDER: You are being monitored",
            "TASK: Defeat 10 enemies (0/10)",
        };

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[PopupSpawner] Initialize called with null root.");
                return;
            }

            _popupWindow  = root.Q<VisualElement>("popup-window");
            _messageLabel = root.Q<Label>("popup-message");
            _popupLayer   = root.Q<VisualElement>("popup-layer");

            if (_popupWindow == null)
                Debug.LogWarning("[PopupSpawner] 'popup-window' not found in HUD.uxml.");

            if (_overloadSystem == null)
                _overloadSystem = FindFirstObjectByType<Gameplay.OverloadSystem>();

            if (_overloadSystem != null) {
                _overloadSystem.OnOverloadTriggered += OnOverloadStart;
                _overloadSystem.OnOverloadEnded     += OnOverloadEnd;
            }

            _currentSpawnRate = _spawnRate;
            _initialized      = true;
        }

        private void OnDestroy() {
            if (_overloadSystem != null) {
                _overloadSystem.OnOverloadTriggered -= OnOverloadStart;
                _overloadSystem.OnOverloadEnded     -= OnOverloadEnd;
            }
        }

        private void Update() {
            if (!_initialized || _overloadSystem == null) return;
            if (_overloadSystem.IsActive) return; // no spawning during overload

            // Angry mode countdown
            if (_angryTimer > 0f) {
                _angryTimer -= Time.deltaTime;
                _currentSpawnRate = _spawnRate * _angryMultiplier;
            } else {
                _currentSpawnRate = _spawnRate;
            }

            // Spawn timer
            _spawnTimer += Time.deltaTime;
            float interval = 1f / _currentSpawnRate;
            if (_spawnTimer >= interval) {
                _spawnTimer = 0f;
                SpawnRandomPopup();
            }

            // Gauge accumulation while popup visible
            if (_popupWindow != null && !_popupWindow.ClassListContains("hidden"))
                _overloadSystem.AddGaugePerSecond(_gaugePerSecond);
        }

        private void SpawnRandomPopup() {
            if (_popupWindow == null || _messageLabel == null) return;

            string message = PopupMessages[Random.Range(0, PopupMessages.Length)];
            _messageLabel.text = message;

            _popupWindow.RemoveFromClassList("hidden");
            _popupWindow.style.opacity = 1f;

            RandomizePosition();
            StartCoroutine(AutoDismiss(_displayDuration));
        }

        /// <summary>Public entry point for external callers (e.g. BossController barrage).</summary>
        public void ForceSpawnPopup() => SpawnRandomPopup();

        private void RandomizePosition() {
            if (_popupWindow == null || _popupLayer == null) return;

            float screenW = _popupLayer.worldBound.width;
            float screenH = _popupLayer.worldBound.height;

            // Fallback to Screen dimensions if worldBound not ready
            if (screenW < 1f) screenW = Screen.width;
            if (screenH < 1f) screenH = Screen.height;

            float mx = 20f;
            float my = 20f;
            float x = Random.Range(mx, Mathf.Max(mx, screenW - mx - 360f));
            float y = Random.Range(my, Mathf.Max(my, screenH - my - 120f));

            _popupWindow.style.left = new StyleLength(new Length(x, LengthUnit.Pixel));
            _popupWindow.style.top  = new StyleLength(new Length(y, LengthUnit.Pixel));
        }

        private IEnumerator AutoDismiss(float delay) {
            yield return new WaitForSeconds(delay);
            if (_popupWindow != null && !_popupWindow.ClassListContains("hidden")) {
                _popupWindow.AddToClassList("hidden");
                _popupWindow.style.opacity = 0f;
            }
        }

        private void OnOverloadStart() {
            StopAllCoroutines();
            if (_popupWindow != null) {
                _popupWindow.AddToClassList("hidden");
                _popupWindow.style.opacity = 0f;
            }
        }

        private void OnOverloadEnd() {
            _angryTimer   = _angryDuration;
            _spawnTimer   = 0f; // spawn immediately after overload
        }
    }
}
