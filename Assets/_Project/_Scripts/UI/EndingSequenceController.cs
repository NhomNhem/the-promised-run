using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ThePromisedRun.UI {
    /// <summary>
    /// EndingSequenceController — plays the ending cutscene after boss death.
    /// Receives VisualElement root from HUDManager (shared UIDocument).
    ///
    /// Sequence (minimal game-jam ending):
    ///   1) Fade to black
    ///   2) Fade in ending text slowly
    ///   3) Hold text
    ///   4) Fade out to black
    ///   5) Return to MainMenu
    /// </summary>
    public class EndingSequenceController : MonoBehaviour {
        private static readonly string[] EndingLines = {
            "ENDING",
            "Bạn tìm thấy nơi hệ thống ở đó.",
            "Nó vẫn spam lời khuyên. Vẫn gào thét vào mic.",
            "Không biết bạn đã đến.",
            "Bạn cầm cây kiếm.",
            "Một cú swing.",
            "Tất cả yên tĩnh.",
            "Không còn lời khuyên nữa."
        };

        private VisualElement _endingRoot;
        private Label         _endingText;
        private VisualElement _screenFade;
        private VisualElement _popupWindow;
        private Label         _popupMessage;
        private bool          _playingEnding;

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[EndingSequenceController] Initialize called with null root.");
                return;
            }

            _endingRoot = root.Q<VisualElement>("ending-root");
            _endingText = root.Q<Label>("ending-text");
            _screenFade = root.Q<VisualElement>("screen-fade");
            _popupWindow = root.Q<VisualElement>("popup-window");
            _popupMessage = root.Q<Label>("popup-message");

            _endingRoot?.AddToClassList("hidden");

            // screen-fade starts transparent (not hidden — needs to be visible for fades)
            if (_screenFade != null) {
                _screenFade.RemoveFromClassList("hidden");
                _screenFade.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
                _screenFade.pickingMode = PickingMode.Ignore;
            }

            if (_endingText != null)
                _endingText.style.opacity = 0f;
        }

        /// <summary>Trigger ending sequence. Called by BossController on death.</summary>
        public void PlayEnding() {
            if (_playingEnding) return;
            StartCoroutine(EndingSequence());
        }

        private IEnumerator EndingSequence() {
            _playingEnding = true;
            Debug.Log("[EndingSequence] Bắt đầu...");

            ShowEndingPopup("KẺ KIẾN TRÚC ĐÃ GỤC NGÃ.\nHỆ THỐNG ĐANG TẮT...");
            yield return new WaitForSecondsRealtime(2.5f);
            HideEndingPopup();

            // 1) Black fade
            yield return FadeScreenTo(new Color(0f, 0f, 0f, 1f), 1f);
            yield return new WaitForSecondsRealtime(1f);
            // screen-fade is topmost; make it transparent so ending text is visible.
            if (_screenFade != null)
                _screenFade.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));

            // 2) Text fade-in
            ShowText(string.Empty, 52);
            yield return PlayEndingTextSequence();

            // 4) Fade out text and keep black screen
            yield return FadeTextTo(0f, 1f);
            yield return FadeScreenTo(new Color(0f, 0f, 0f, 1f), 2f);

            // 5) Back to MainMenu
            Debug.Log("[EndingSequence] Hoàn tất. Đang tải MainMenu.");
            try {
                SceneManager.LoadScene(0); // Scene_MainMenu is usually build index 0
            } catch {
                SceneManager.LoadScene("Scene_MainMenu");
            }
        }

        private void ShowText(string message, int fontSize) {
            if (_endingText == null || _endingRoot == null) return;
            _endingText.text = message;
            _endingText.style.fontSize = new StyleLength(new Length(fontSize, LengthUnit.Pixel));
            _endingRoot.RemoveFromClassList("hidden");
        }

        private void HideText() {
            _endingRoot?.AddToClassList("hidden");
        }

        private IEnumerator FadeTextTo(float targetOpacity, float duration) {
            if (_endingText == null) yield break;

            float startOpacity = _endingText.resolvedStyle.opacity;
            float elapsed = 0f;

            while (elapsed < duration) {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _endingText.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, t);
                yield return null;
            }

            _endingText.style.opacity = targetOpacity;
        }

        private IEnumerator PlayEndingTextSequence() {
            if (_endingText == null) yield break;

            _endingText.text = string.Empty;
            _endingText.style.opacity = 1f;

            string built = string.Empty;
            for (int i = 0; i < EndingLines.Length; i++) {
                if (i > 0) built += "\n";
                built += EndingLines[i];
                _endingText.text = built;

                if (i == 0) {
                    yield return new WaitForSecondsRealtime(1.1f);
                } else if (i >= 4) {
                    yield return new WaitForSecondsRealtime(0.85f);
                } else {
                    yield return new WaitForSecondsRealtime(1.0f);
                }
            }

            yield return new WaitForSecondsRealtime(2.0f);
        }

        private void ShowEndingPopup(string message) {
            if (_popupWindow == null || _popupMessage == null) return;
            _popupMessage.text = message;
            _popupWindow.RemoveFromClassList("hidden");
        }

        private void HideEndingPopup() {
            _popupWindow?.AddToClassList("hidden");
        }

        private IEnumerator FadeScreenTo(Color targetColor, float duration) {
            if (_screenFade == null) yield break;

            Color startColor = _screenFade.style.backgroundColor.value;
            float elapsed    = 0f;

            while (elapsed < duration) {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _screenFade.style.backgroundColor = new StyleColor(Color.Lerp(startColor, targetColor, t));
                yield return null;
            }

            _screenFade.style.backgroundColor = new StyleColor(targetColor);
        }
    }
}
