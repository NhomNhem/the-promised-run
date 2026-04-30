using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using OpenUtility.Data;

namespace ThePromisedRun.UI {
    /// <summary>
    /// OverloadFlashEffect — 1-frame white flash when Overload triggers.
    /// Receives VisualElement root from HUDManager (shared UIDocument).
    /// Uses screen-fade element for the flash.
    ///
    /// Sequence:
    ///   Frame 1: screen-fade → white (opacity 1)
    ///   Frame 2-3: fade back to transparent (0.1s)
    /// </summary>
    public class OverloadFlashEffect : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float _flashDuration  = 0.08f; // 1-frame flash
        [SerializeField] private float _fadeDuration   = 0.15f; // fade back

        [Header("ScriptableVariables")]
        [SerializeField] private ScriptableBool _overloadStateVar;

        private VisualElement _screenFade;
        private bool          _initialized;
        private Coroutine     _flashCoroutine;

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) return;
            _screenFade  = root.Q<VisualElement>("screen-fade");
            _initialized = true;

            if (_overloadStateVar != null)
                _overloadStateVar.ValueChanged.AddListener(OnOverloadStateChanged);
            else
                Debug.LogWarning("[OverloadFlashEffect] _overloadStateVar not assigned.");
        }

        private void OnDestroy() {
            _overloadStateVar?.ValueChanged.RemoveListener(OnOverloadStateChanged);
        }

        private void OnOverloadStateChanged(bool isOverloaded) {
            if (isOverloaded) TriggerFlash();
        }

        public void TriggerFlash() {
            if (!_initialized || _screenFade == null) return;
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine() {
            // Instant white
            _screenFade.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 1f));

            yield return new WaitForSecondsRealtime(_flashDuration);

            // Fade back to transparent
            float elapsed = 0f;
            while (elapsed < _fadeDuration) {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _fadeDuration);
                float alpha = Mathf.Lerp(1f, 0f, t);
                _screenFade.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, alpha));
                yield return null;
            }

            _screenFade.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
        }
    }
}
