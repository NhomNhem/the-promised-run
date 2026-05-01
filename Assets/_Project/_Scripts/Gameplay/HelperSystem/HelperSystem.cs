using UnityEngine;
using UnityEngine.Events;
using OpenUtility.Data;

namespace ThePromisedRun.Gameplay.HelperSystem {
    /// <summary>
    /// The Helper System — the game's main antagonist.
    /// Uses Priority Queue Logic (GDD §5.1) — not random.
    /// System senses player state → picks optimal popup to ruin that moment.
    ///
    /// Priority:
    ///   Jumping (falling)  → CoverLandingSpot (80%) or WarningAfterFall (20%)
    ///   In combat          → FakeQuest (60%) or FakeLevelUp (40%)
    ///   AFK > 2s           → AFKWarning (100%)
    ///   HP < 30%           → FakeHealRecommend (70%) or MimicSpawn (30%)
    ///   Post-Overload      → SystemRecalibrating (×1.5 rate for 10s)
    ///   Near exit          → ExitBlockQuest
    ///   Default            → random misleading message
    /// </summary>
    public class HelperSystem : MonoBehaviour {
        #region Popup Types
        public enum PopupType {
            LateWarning,
            FakeQuest,
            FakeLevelUp,
            AFKWarning,
            FakeHealRecommend,
            SystemRecalibrating,
            ExitBlockQuest,
            Generic,
        }
        #endregion

        [Header("Config")]
        [SerializeField] private HelperSystemConfig _config;

        [Header("References")]
        [SerializeField] private PlayerController _player;
        [SerializeField] private Combat.PlayerHealth _playerHealth;

        [Header("ScriptableVariables")]
        [SerializeField] private ScriptableBool  _overloadStateVar;  // written by PlayerController
        [SerializeField] private ScriptableFloat _healthVar;         // read for HP check
        [SerializeField] private ScriptableString _popupMessageVar;  // written here → PopupUI reads
        [SerializeField] private ScriptableBool   _popupMutedVar;    // written here → PopupUI reads

        [Header("Events")]
        public UnityEvent              OnPopupSpawn     = new UnityEvent();
        public UnityEvent              OnPopupDismissed = new UnityEvent();
        public UnityEvent<string>      OnMessageChanged = new UnityEvent<string>();
        public UnityEvent<PopupType>   OnPopupType      = new UnityEvent<PopupType>();
        public UnityEvent              OnSystemMuted    = new UnityEvent();
        public UnityEvent              OnSystemRestored = new UnityEvent();

        // State
        private float _nextPopupTime;
        private bool  _isMuted;
        private float _idleTimer;
        private float _postOverloadTimer;
        private bool  _isPostOverload;
        private float _popupOnScreenTimer;
        private bool  _popupActive;

        // AFK threshold
        private const float AFKThreshold    = 2f;
        private const float PostOverloadTime = 10f;

        #region Popup Messages by Type
        private static readonly string[] LateWarningMsgs = {
            "NHẢY!", "CHẠY!", "COI CHỪNG!", "NÉ ĐÒN!", "CẢNH BÁO: Nguy hiểm (3 giây trước)"
        };
        private static readonly string[] FakeQuestMsgs = {
            "NHIỆM VỤ MỚI: Thu thập 3 Đồng Ma Ảnh",
            "MỤC TIÊU: Tìm Nấm Thiêng",
            "NHIỆM VỤ: Hạ 10 kẻ địch (0/10)",
            "NHIỆM VỤ PHỤ: Quay lại điểm bắt đầu",
        };
        private static readonly string[] FakeLevelUpMsgs = {
            "LÊN CẤP! +5 TRÍ TUỆ",
            "THÀNH TỰU MỞ KHÓA: Hít thở",
            "THƯỞNG: +0 sát thương trong 0 giây",
            "CÓ ĐIỂM KỸ NĂNG (không tìm thấy cây kỹ năng)",
        };
        private static readonly string[] AFKMsgs = {
            "Tự động đăng xuất sau 5 giây...",
            "CẢNH BÁO: Phát hiện không hoạt động",
            "PHIÊN SẮP HẾT HẠN: Di chuyển hoặc sẽ bị loại",
        };
        private static readonly string[] FakeHealMsgs = {
            "KHUYÊN DÙNG: Thuốc hồi máu!",
            "MẸO: Có vật phẩm hồi máu ở gần",
            "CẢNH BÁO: Máu thấp — uống thuốc (nhưng không có thuốc)",
        };
        private static readonly string[] RecalibrateMsgs = {
            "HỆ THỐNG ĐANG HIỆU CHỈNH...",
            "LỖI: Hành vi anh hùng ngoài dự kiến",
            "ĐANG TÍNH LẠI chiến thuật tối ưu...",
            "PHÁT HIỆN DỊ THƯỜNG. Đang điều chỉnh tham số.",
        };
        private static readonly string[] ExitBlockMsgs = {
            "ĐÃ MỞ NHIỆM VỤ MỚI! Hãy khám phá sâu hơn.",
            "KHOAN — phát hiện mục tiêu chưa hoàn thành",
            "THÀNH TỰU: Đã khám phá 0% khu vực. Quay lại ngay.",
        };
        private static readonly string[] GenericMsgs = {
            "MẸO: Cố gắng đừng chết",
            "GỢI Ý: Lối ra nằm đâu đó",
            "CHÚC MỪNG! (cho hành động trước đó của bạn)",
            "NHẮC NHỞ: Bạn đang bị theo dõi",
            "HỆ THỐNG: Mọi thứ ổn. Hãy tiếp tục bình thường.",
        };
        #endregion

        private void Awake() {
            if (_player == null)       _player       = FindFirstObjectByType<PlayerController>();
            if (_playerHealth == null) _playerHealth = FindFirstObjectByType<Combat.PlayerHealth>();

            if (_config == null)
                Debug.LogWarning("[HelperSystem] No config assigned — using defaults.");

            // Subscribe to overload state via ScriptableVariable (scene-safe)
            if (_overloadStateVar != null)
                _overloadStateVar.ValueChanged.AddListener(OnOverloadStateChanged);
            else {
                // Fallback: direct UnityEvent subscription (single-scene only)
                _player?.OnOverloadStarted.AddListener(OnOverloadStarted);
                _player?.OnOverloadEnded.AddListener(OnOverloadEnded);
            }

            ScheduleNextPopup();
        }

        private void OnDestroy() {
            if (_overloadStateVar != null)
                _overloadStateVar.ValueChanged.RemoveListener(OnOverloadStateChanged);
            else if (_player != null) {
                _player.OnOverloadStarted.RemoveListener(OnOverloadStarted);
                _player.OnOverloadEnded.RemoveListener(OnOverloadEnded);
            }
        }

        private void Update() {
            if (_isMuted) return;

            // Track idle time
            if (_player != null && _player.Input != null) {
                bool moving = _player.Input.MoveInput.sqrMagnitude > 0.01f;
                _idleTimer = moving ? 0f : _idleTimer + Time.deltaTime;
            }

            // Post-overload angry timer
            if (_isPostOverload) {
                _postOverloadTimer -= Time.deltaTime;
                if (_postOverloadTimer <= 0f) _isPostOverload = false;
            }

            // Popup on-screen chaos tick (+5/sec)
            if (_popupActive) {
                _popupOnScreenTimer += Time.deltaTime;
                if (_popupOnScreenTimer >= 1f) {
                    _popupOnScreenTimer = 0f;
                    _player?.AddChaos(5f, ChaosSource.SystemInterference);
                }
            }

            if (Time.time >= _nextPopupTime)
                SpawnPopup();
        }

        private void SpawnPopup() {
            ScheduleNextPopup();

            PopupType type = SelectPopupType();
            string msg     = SelectMessage(type);

            OnMessageChanged.Invoke(msg);
            OnPopupType.Invoke(type);
            OnPopupSpawn.Invoke();

            // Write to ScriptableVariable — PopupUI subscribes to this
            _popupMessageVar?.SetValue(msg);

            _popupActive        = true;
            _popupOnScreenTimer = 0f;

            _player?.AddChaos(GetChaos(_config?.chaosOnPopupSpawn ?? 8f), ChaosSource.SystemInterference);
        }

        /// <summary>Priority Queue — picks popup type based on player state.</summary>
        private PopupType SelectPopupType() {
            if (_player == null) return PopupType.Generic;

            // AFK > 2s → 100% AFKWarning
            if (_idleTimer > AFKThreshold)
                return PopupType.AFKWarning;

            // Falling (jumping over pit)
            bool isFalling = !_player.IsGrounded &&
                             _player.Rb != null &&
                             _player.Rb.linearVelocity.y < -2f;
            if (isFalling)
                return Random.value < 0.8f ? PopupType.LateWarning : PopupType.LateWarning;

            // In combat (attacking or enemy nearby)
            bool inCombat = _player.GetNearestEnemy() != null;
            if (inCombat)
                return Random.value < 0.6f ? PopupType.FakeQuest : PopupType.FakeLevelUp;

            // Low HP — read from ScriptableVariable if available, else fallback to PlayerHealth
            float hpNorm;
            if (_healthVar != null)
                hpNorm = Mathf.Clamp01(_healthVar.GetValue() / 100f);
            else
                hpNorm = _playerHealth != null ? _playerHealth.HealthNorm : 1f;
            if (hpNorm < 0.3f)
                return Random.value < 0.7f ? PopupType.FakeHealRecommend : PopupType.FakeHealRecommend;

            // Post-overload
            if (_isPostOverload)
                return PopupType.SystemRecalibrating;

            return PopupType.Generic;
        }

        private string SelectMessage(PopupType type) {
            return type switch {
                PopupType.LateWarning        => Pick(LateWarningMsgs),
                PopupType.FakeQuest          => Pick(FakeQuestMsgs),
                PopupType.FakeLevelUp        => Pick(FakeLevelUpMsgs),
                PopupType.AFKWarning         => Pick(AFKMsgs),
                PopupType.FakeHealRecommend  => Pick(FakeHealMsgs),
                PopupType.SystemRecalibrating => Pick(RecalibrateMsgs),
                PopupType.ExitBlockQuest     => Pick(ExitBlockMsgs),
                _                            => Pick(GenericMsgs),
            };
        }

        private static string Pick(string[] arr) => arr[Random.Range(0, arr.Length)];

        private void ScheduleNextPopup() {
            float min = _config?.minInterval ?? 3f;
            float max = _config?.maxInterval ?? 7f;
            // Post-overload: ×1.5 spawn rate = shorter interval
            if (_isPostOverload) { min *= 0.67f; max *= 0.67f; }
            _nextPopupTime = Time.time + Random.Range(min, max);
        }

        /// <summary>Call when popup is dismissed by player.</summary>
        public void DismissPopup() {
            _popupActive = false;
            OnPopupDismissed.Invoke();
        }

        public void NotifyPlayerObstructed() =>
            _player?.AddChaos(GetChaos(_config?.chaosOnPlayerObstructed ?? 12f), ChaosSource.SystemInterference);

        public void NotifyPlayerFailed() =>
            _player?.AddChaos(GetChaos(_config?.chaosOnPlayerFail ?? 20f), ChaosSource.SystemInterference);

        private float GetChaos(float base_) =>
            base_ * (_config?.aggressivenessMultiplier ?? 1f);

        private void OnOverloadStarted() {
            _isMuted     = true;
            _popupActive = false;
            _popupMutedVar?.SetValue(true);  // notify PopupUI to hide
            OnSystemMuted.Invoke();
        }

        private void OnOverloadEnded() {
            _isMuted           = false;
            _isPostOverload    = true;
            _postOverloadTimer = PostOverloadTime;
            _popupMutedVar?.SetValue(false); // notify PopupUI to resume
            ScheduleNextPopup();
            OnSystemRestored.Invoke();
        }

        /// <summary>Called when overloadState ScriptableBool changes.</summary>
        private void OnOverloadStateChanged(bool isOverloaded) {
            if (isOverloaded) OnOverloadStarted();
            else              OnOverloadEnded();
        }

        public void SetAggressiveness(float multiplier) {
            if (_config != null) _config.aggressivenessMultiplier = multiplier;
        }
    }
}
