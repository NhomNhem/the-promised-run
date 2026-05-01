using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Death screen — hiển thị "THUA CUỘC" khi Player chết.
    /// Chờ người chơi nhấn "CHƠI LẠI" để respawn tại checkpoint gần nhất.
    /// Receives its VisualElement root from HUDManager (shared UIDocument).
    /// </summary>
    public class DeathScreen : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float _fadeInDuration  = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.3f;

        [Header("Respawn")]
        [SerializeField] private Gameplay.CheckpointSystem _checkpoint;

        private static readonly string[] CauseMessages = {
            "NGUYÊN NHÂN: Lỗi trọng lực (không liên quan đến Hệ thống)",
            "NGUYÊN NHÂN: Lỗi hiệu chỉnh kiếm — đang điều tra",
            "NGUYÊN NHÂN: Lỗi người dùng — không phải lỗi Hệ thống",
            "NGUYÊN NHÂN: Môi trường thù địch (ngoài dự kiến)",
            "NGUYÊN NHÂN: Dị thường thống kê. Đang chạy lại dữ liệu.",
            "NGUYÊN NHÂN: Nguy hiểm môi trường (tình trạng có sẵn)",
            "NGUYÊN NHÂN: Lỗi thiết bị — bảo hành đã hết hạn",
            "NGUYÊN NHÂN: Lệch thời gian — không phải lỗi của chúng tôi",
        };

        private VisualElement _deathRoot;
        private Label         _statusLabel;
        private Label         _causeLabel;
        private Button        _btnPlayAgain;
        private CancellationTokenSource _cts;

        /// <summary>Called by HUDManager with the shared UIDocument root.</summary>
        public void Initialize(VisualElement root) {
            if (root == null) {
                Debug.LogWarning("[DeathScreen] Initialize called with null root.");
                return;
            }

            _deathRoot    = root.Q<VisualElement>("death-root");
            _statusLabel  = root.Q<Label>("status-text");
            _causeLabel   = root.Q<Label>("cause-text");
            _btnPlayAgain = root.Q<Button>("btn-play-again");

            _deathRoot?.AddToClassList("hidden");

            // Wire "CHƠI LẠI" button
            if (_btnPlayAgain != null)
                _btnPlayAgain.RegisterCallback<ClickEvent>(_ => OnPlayAgainClicked());

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

        /// <summary>
        /// Hiển thị màn hình thua cuộc. Gọi từ PlayerHealth.OnDeathUnity.
        /// Dừng gameplay (timeScale = 0) và chờ người chơi nhấn "CHƠI LẠI".
        /// </summary>
        public void Show() {
            if (_deathRoot == null) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _ = DeathSequenceAsync(_cts.Token);
        }

        private async Awaitable DeathSequenceAsync(CancellationToken ct) {
            // Set text
            if (_statusLabel != null)
                _statusLabel.text = "THUA CUỘC";

            if (_causeLabel != null)
                _causeLabel.text = CauseMessages[Random.Range(0, CauseMessages.Length)];

            // Dừng gameplay
            Time.timeScale = 0f;

            _deathRoot.RemoveFromClassList("hidden");
            _deathRoot.style.opacity = 0f;

            // Fade in — dùng unscaled time vì timeScale = 0
            await FadeAsync(0f, 1f, _fadeInDuration, ct);
            // Không auto-respawn — chờ người chơi nhấn btn-play-again
        }

        /// <summary>Xử lý khi người chơi nhấn "CHƠI LẠI".</summary>
        private void OnPlayAgainClicked() {
            if (_checkpoint == null) {
                Debug.LogWarning("[DeathScreen] CheckpointSystem not assigned — respawn skipped.");
            } else {
                _checkpoint.Respawn();
            }

            _ = HideAndResumeAsync();
        }

        /// <summary>Fade out overlay và resume gameplay.</summary>
        private async Awaitable HideAndResumeAsync() {
            // Cancel sequence đang chạy (nếu có)
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            await FadeAsync(1f, 0f, _fadeOutDuration, _cts.Token);

            if (_deathRoot != null)
                _deathRoot.AddToClassList("hidden");

            // Resume gameplay
            Time.timeScale = 1f;
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
