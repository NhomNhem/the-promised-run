using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Death screen — GDD §9.4.
    /// Shows "HERO #47 PERFORMANCE REVIEW" with System Blame messages.
    /// System never admits fault. Always blames external factors.
    /// Fades in, waits, then triggers respawn.
    /// </summary>
    public class DeathScreen : MonoBehaviour {
        [Header("References")]
        [SerializeField] private CanvasGroup    _canvasGroup;
        [SerializeField] private TextMeshProUGUI _causeText;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Config")]
        [SerializeField] private float _fadeInDuration  = 0.5f;
        [SerializeField] private float _displayDuration = 3.0f;
        [SerializeField] private float _fadeOutDuration = 0.3f;

        [Header("Respawn")]
        [SerializeField] private Gameplay.CheckpointSystem _checkpoint;

        // System Blame death messages — System never admits fault
        private static readonly string[] CauseMessages = {
            "CAUSE: Gravity malfunction (unrelated to System)",
            "CAUSE: Sword calibration error — investigating",
            "CAUSE: User error — not System error",
            "CAUSE: Hostile environment (unexpected)",
            "CAUSE: Statistical anomaly. Rerunning data.",
            "CAUSE: Environmental hazard (pre-existing condition)",
            "CAUSE: Equipment failure — warranty void",
            "CAUSE: Temporal misalignment — not our fault",
        };

        private void Awake() {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_checkpoint == null)  _checkpoint  = FindFirstObjectByType<Gameplay.CheckpointSystem>();
            gameObject.SetActive(false);
        }

        /// <summary>Show death screen then respawn. Call from PlayerHealth.OnDeathUnity.</summary>
        public void Show() {
            gameObject.SetActive(true);
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence() {
            // Set text
            if (_statusText != null)
                _statusText.text = "HERO #47 PERFORMANCE REVIEW\n\nStatus: TERMINATED\n\nResuming from last checkpoint...\n\"ANOMALY DETECTED. Restarting session.\"";

            if (_causeText != null)
                _causeText.text = CauseMessages[Random.Range(0, CauseMessages.Length)];

            // Fade in
            yield return Fade(0f, 1f, _fadeInDuration);

            // Display
            yield return new WaitForSecondsRealtime(_displayDuration);

            // Respawn
            _checkpoint?.Respawn();

            // Fade out
            yield return Fade(1f, 0f, _fadeOutDuration);

            gameObject.SetActive(false);
        }

        private IEnumerator Fade(float from, float to, float duration) {
            float t = 0f;
            while (t < duration) {
                t += Time.unscaledDeltaTime;
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            if (_canvasGroup != null) _canvasGroup.alpha = to;
        }
    }
}
