using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ThePromisedRun.UI {
    /// <summary>
    /// EndingSequenceController — plays the ending cutscene after boss death.
    /// Receives VisualElement root from HUDManager (shared UIDocument).
    ///
    /// Sequence:
    ///   T=0s:  Fade to black (1s)
    ///   T=1s:  Silence 2s
    ///   T=3s:  "SYSTEM OFFLINE. HERO #47 DESIGNATION: GRADUATE." (1s)
    ///   T=4s:  Hide text
    ///   T=5s:  Footstep audio (8s)
    ///   T=13s: Fade to white (2s)
    ///   T=15s: Wait 1s
    ///   T=16s: "HERO #47 WAS THE LAST." (3s, large font)
    ///   T=19s: Fade to black (3s)
    ///   T=22s: Wait 2s
    ///   T=24s: Return to MainMenu
    /// </summary>
    public class EndingSequenceController : MonoBehaviour {
        [Header("Audio")]
        [SerializeField] private AudioClip _footstepClip;
        [SerializeField] [Range(0f,1f)] private float _footstepVolume = 0.8f;

        private VisualElement _endingRoot;
        private Label         _endingText;
        private VisualElement _screenFade;
        private AudioSource   _audioSource;
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

            _endingRoot?.AddToClassList("hidden");

            // screen-fade starts transparent (not hidden — needs to be visible for fades)
            if (_screenFade != null) {
                _screenFade.RemoveFromClassList("hidden");
                _screenFade.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>Trigger ending sequence. Called by BossController on death.</summary>
        public void PlayEnding() {
            if (_playingEnding) return;
            StartCoroutine(EndingSequence());
        }

        private IEnumerator EndingSequence() {
            _playingEnding = true;
            Debug.Log("[EndingSequence] Starting...");

            // T=0s: Fade to black
            yield return FadeScreenTo(new Color(0f, 0f, 0f, 1f), 1f);

            // T=1s: Silence
            yield return new WaitForSeconds(2f);

            // T=3s: First message
            ShowText("SYSTEM OFFLINE.\nHERO #47 DESIGNATION: GRADUATE.", 60);
            yield return new WaitForSeconds(3f);

            // T=6s: Hide text
            HideText();
            yield return new WaitForSeconds(1f);

            // T=7s: Footstep audio
            if (_footstepClip != null && _audioSource != null) {
                _audioSource.clip   = _footstepClip;
                _audioSource.volume = _footstepVolume;
                _audioSource.Play();
                yield return new WaitForSeconds(_footstepClip.length);
            } else {
                yield return new WaitForSeconds(4f);
            }

            // Fade to white
            yield return FadeScreenTo(new Color(1f, 1f, 1f, 1f), 2f);
            yield return new WaitForSeconds(1f);

            // Final text
            ShowText("HERO #47 WAS THE LAST.", 80);
            yield return new WaitForSeconds(4f);

            // Fade to black
            HideText();
            yield return FadeScreenTo(new Color(0f, 0f, 0f, 1f), 3f);
            yield return new WaitForSeconds(2f);

            // Return to MainMenu
            Debug.Log("[EndingSequence] Complete. Loading MainMenu.");
            SceneManager.LoadScene(0); // Scene_MainMenu is build index 0
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

        private IEnumerator FadeScreenTo(Color targetColor, float duration) {
            if (_screenFade == null) yield break;

            Color startColor = _screenFade.style.backgroundColor.value;
            float elapsed    = 0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _screenFade.style.backgroundColor = new StyleColor(Color.Lerp(startColor, targetColor, t));
                yield return null;
            }

            _screenFade.style.backgroundColor = new StyleColor(targetColor);
        }
    }
}
