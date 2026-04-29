using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Threading;
using OpenUtility.Data;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Popup UI — GDD §5.2. CRT monospace green-on-black aesthetic.
    /// Receives its VisualElement root from HUDManager (shared UIDocument).
    ///
    /// Decoupled from HelperSystem via ScriptableVariables:
    ///   _popupMessageVar (ScriptableString) — HelperSystem writes, PopupUI reads
    ///   _popupMutedVar   (ScriptableBool)   — HelperSystem writes, PopupUI reads
    ///
    /// No FindFirstObjectByType — safe for multi-scene additive loading.
    /// </summary>
    public class PopupUI : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float   _displayDuration = 4f;
        [SerializeField] private float   _fadeTime        = 0.15f;

        [Header("Positioning")]
        [SerializeField] private Vector2 _marginPercent = new Vector2(0.1f, 0.1f);

        [Header("ScriptableVariables")]
        [SerializeField] private ScriptableString _popupMessageVar; // HelperSystem → PopupUI
        [SerializeField] private ScriptableBool   _popupMutedVar;   // HelperSystem → PopupUI

        private VisualElement _popupWindow;
        private Label         _messageLabel;

        private bool  _visible;
        private float _dismissTimer;
        private CancellationTokenSource _cts;

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[PopupUI] Initialize called with null root.");
                return;
            }

            _popupWindow  = root.Q<VisualElement>("popup-window");
            _messageLabel = root.Q<Label>("popup-message");

            if (_popupWindow == null)
                Debug.LogWarning("[PopupUI] 'popup-window' element not found in HUD root.");

            _popupWindow?.AddToClassList("hidden");
            if (_popupWindow != null) _popupWindow.style.opacity = 0f;

            // Subscribe to ScriptableVariable events
            if (_popupMessageVar != null)
                _popupMessageVar.ValueChanged.AddListener(ShowPopup);
            else
                Debug.LogWarning("[PopupUI] _popupMessageVar not assigned. Popup will not receive messages.");

            if (_popupMutedVar != null)
                _popupMutedVar.ValueChanged.AddListener(OnMutedChanged);
        }

        private void OnDestroy() {
            _cts?.Cancel();
            _cts?.Dispose();
            _popupMessageVar?.ValueChanged.RemoveListener(ShowPopup);
            _popupMutedVar?.ValueChanged.RemoveListener(OnMutedChanged);
        }

        private void Update() {
            if (_visible && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                Dismiss();

            if (_visible) {
                _dismissTimer -= Time.deltaTime;
                if (_dismissTimer <= 0f) Dismiss();
            }
        }

        private void ShowPopup(string message) {
            if (_popupWindow == null) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            if (_messageLabel != null) _messageLabel.text = message;
            RandomizePosition();

            _dismissTimer = _displayDuration;
            _visible      = true;

            _popupWindow.RemoveFromClassList("hidden");
            _ = FadeAsync(0f, 1f, _fadeTime, _cts.Token);
        }

        private void OnMutedChanged(bool muted) {
            if (muted) HideImmediate();
        }

        private void HideImmediate() {
            _cts?.Cancel();
            _visible = false;
            if (_popupWindow != null) {
                _popupWindow.style.opacity = 0f;
                _popupWindow.AddToClassList("hidden");
            }
        }

        private void Dismiss() {
            if (!_visible) return;
            _visible = false;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _ = FadeOutAndHideAsync(_cts.Token);
        }

        private async Awaitable FadeOutAndHideAsync(CancellationToken ct) {
            await FadeAsync(1f, 0f, _fadeTime, ct);
            if (!ct.IsCancellationRequested)
                _popupWindow?.AddToClassList("hidden");
        }

        private async Awaitable FadeAsync(float from, float to, float duration, CancellationToken ct) {
            if (_popupWindow == null) return;

            float elapsed = 0f;
            _popupWindow.style.opacity = from;

            while (elapsed < duration) {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                _popupWindow.style.opacity = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                try { await Awaitable.NextFrameAsync(ct); }
                catch (System.OperationCanceledException) { return; }
            }

            _popupWindow.style.opacity = to;
        }

        private void RandomizePosition() {
            if (_popupWindow == null) return;
            float screenW = Screen.width;
            float screenH = Screen.height;
            float mx = screenW * _marginPercent.x;
            float my = screenH * _marginPercent.y;
            float x = Random.Range(mx, Mathf.Max(mx, screenW - mx - 360f));
            float y = Random.Range(my, Mathf.Max(my, screenH - my - 120f));
            _popupWindow.style.left = new StyleLength(new Length(x, LengthUnit.Pixel));
            _popupWindow.style.top  = new StyleLength(new Length(y, LengthUnit.Pixel));
        }
    }
}
