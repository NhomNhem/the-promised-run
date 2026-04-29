using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThePromisedRun.Gameplay.HelperSystem;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Popup UI — GDD §5.2.
    /// Monospace green-on-black CRT aesthetic.
    /// Appears at random screen position, auto-dismisses after duration.
    /// Player can close with F/RB (0.5s animation).
    /// </summary>
    public class PopupUI : MonoBehaviour {
        [Header("References")]
        [SerializeField] private CanvasGroup     _canvasGroup;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private RectTransform   _popupRect;
        [SerializeField] private HelperSystem    _helperSystem;

        [Header("Config")]
        [SerializeField] private float _displayDuration = 4f;
        [SerializeField] private float _fadeTime        = 0.15f;

        [Header("Positioning")]
        [SerializeField] private Vector2 _marginPercent = new Vector2(0.1f, 0.1f);

        private bool  _visible;
        private float _dismissTimer;

        private void Awake() {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_helperSystem == null) _helperSystem = FindFirstObjectByType<HelperSystem>();

            if (_helperSystem != null) {
                _helperSystem.OnMessageChanged.AddListener(ShowPopup);
                _helperSystem.OnSystemMuted.AddListener(HideImmediate);
            }

            _canvasGroup.alpha = 0f;
            _visible = false;
        }

        private void OnDestroy() {
            if (_helperSystem != null) {
                _helperSystem.OnMessageChanged.RemoveListener(ShowPopup);
                _helperSystem.OnSystemMuted.RemoveListener(HideImmediate);
            }
        }

        private void Update() {
            // F key or RB to dismiss
            if (_visible && UnityEngine.Input.GetKeyDown(KeyCode.F)) {
                StartCoroutine(DismissRoutine());
            }

            // Auto-dismiss
            if (_visible) {
                _dismissTimer -= Time.deltaTime;
                if (_dismissTimer <= 0f)
                    StartCoroutine(DismissRoutine());
            }
        }

        private void ShowPopup(string message) {
            StopAllCoroutines();
            if (_messageText != null) _messageText.text = message;
            RandomizePosition();
            _dismissTimer = _displayDuration;
            _visible      = true;
            StartCoroutine(FadeIn());
        }

        private void HideImmediate() {
            StopAllCoroutines();
            _visible = false;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }

        private IEnumerator DismissRoutine() {
            _visible = false;
            _helperSystem?.DismissPopup();
            yield return StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn() {
            float t = 0f;
            while (t < _fadeTime) {
                t += Time.deltaTime;
                if (_canvasGroup != null) _canvasGroup.alpha = t / _fadeTime;
                yield return null;
            }
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut() {
            float t = _fadeTime;
            while (t > 0f) {
                t -= Time.deltaTime;
                if (_canvasGroup != null) _canvasGroup.alpha = t / _fadeTime;
                yield return null;
            }
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }

        private void RandomizePosition() {
            if (_popupRect == null) return;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Vector2 size = ((RectTransform)canvas.transform).sizeDelta;
            float mx = size.x * _marginPercent.x;
            float my = size.y * _marginPercent.y;

            float x = Random.Range(-size.x * 0.5f + mx, size.x * 0.5f - mx);
            float y = Random.Range(-size.y * 0.5f + my, size.y * 0.5f - my);
            _popupRect.anchoredPosition = new Vector2(x, y);
        }
    }
}
