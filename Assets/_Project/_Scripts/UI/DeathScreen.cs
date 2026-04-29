using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Death screen — GDD §9.4. "HERO #47 PERFORMANCE REVIEW".
    /// Receives its VisualElement root from HUDManager (shared UIDocument).
    /// Queries death-root, status-text, cause-text from the death-layer.
    /// </summary>
    public class DeathScreen : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float _fadeInDuration  = 0.5f;
        [SerializeField] private float _displayDuration = 3.0f;
        [SerializeField] private float _fadeOutDuration = 0.3f;

        [Header("Respawn")]
        [SerializeField] private Gameplay.CheckpointSystem _checkpoint;

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

        private VisualElement _deathRoot;
        private Label         _statusLabel;
        private Label         _causeLabel;
        private CancellationTokenSource _cts;

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[DeathScreen] Initialize called with null root.");
                return;
            }

            _deathRoot   = root.Q<VisualElement>("death-root");
            _statusLabel = root.Q<Label>("status-text");
            _causeLabel  = root.Q<Label>("cause-text");

            _deathRoot?.AddToClassList("hidden");

            // _checkpoint must be assigned in Inspector — no FindFirstObjectByType
            // (breaks multi-scene additive loading)
            if (_checkpoint == null)
                Debug.LogWarning("[DeathScreen] CheckpointSystem not assigned. Drag it into Inspector. Respawn will be skipped.");
        }

        private void OnDisable() {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        /// <summary>Show death screen then respawn. Call from PlayerHealth.OnDeathUnity.</summary>
        public void Show() {
            if (_deathRoot == null) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _ = DeathSequenceAsync(_cts.Token);
        }

        private async Awaitable DeathSequenceAsync(CancellationToken ct) {
            if (_statusLabel != null)
                _statusLabel.text = "HERO #47 PERFORMANCE REVIEW\n\nStatus: TERMINATED\n\nResuming from last checkpoint...\n\"ANOMALY DETECTED. Restarting session.\"";

            if (_causeLabel != null)
                _causeLabel.text = CauseMessages[Random.Range(0, CauseMessages.Length)];

            _deathRoot.RemoveFromClassList("hidden");
            _deathRoot.style.opacity = 0f;

            await FadeAsync(0f, 1f, _fadeInDuration, ct);
            if (ct.IsCancellationRequested) return;

            try { await Awaitable.WaitForSecondsAsync(_displayDuration, ct); }
            catch (System.OperationCanceledException) { return; }

            _checkpoint?.Respawn();

            await FadeAsync(1f, 0f, _fadeOutDuration, ct);
            if (ct.IsCancellationRequested) return;

            _deathRoot.AddToClassList("hidden");
        }

        private async Awaitable FadeAsync(float from, float to, float duration, CancellationToken ct) {
            if (_deathRoot == null) return;

            float elapsed = 0f;
            _deathRoot.style.opacity = from;

            while (elapsed < duration) {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.unscaledDeltaTime;
                _deathRoot.style.opacity = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                try { await Awaitable.NextFrameAsync(ct); }
                catch (System.OperationCanceledException) { return; }
            }

            _deathRoot.style.opacity = to;
        }
    }
}
