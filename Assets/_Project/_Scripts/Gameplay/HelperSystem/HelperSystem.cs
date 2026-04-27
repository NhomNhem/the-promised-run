using UnityEngine;
using UnityEngine.Events;

namespace ThePromisedRun.Gameplay.HelperSystem {
    /// <summary>
    /// The Helper System — the game's main antagonist.
    /// Spawns popups on a timer, adds chaos on each interference event.
    /// 
    /// Core loop: System interferes → Chaos rises → Overload triggers → System muted.
    /// The more the system interferes, the faster the player can overload it.
    /// </summary>
    public class HelperSystem : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private HelperSystemConfig _config;

        [Header("References")]
        [SerializeField] private PlayerController _player;

        [Header("Events — subscribe UI/VFX here")]
        public UnityEvent          OnPopupSpawn      = new UnityEvent();
        public UnityEvent          OnPopupDismissed  = new UnityEvent();
        public UnityEvent<string>  OnMessageChanged  = new UnityEvent<string>();
        public UnityEvent          OnSystemMuted     = new UnityEvent();
        public UnityEvent          OnSystemRestored  = new UnityEvent();

        private float _nextPopupTime;
        private bool  _isMuted;

        // Delayed/misleading messages — the "Helper" is always right but always too late
        private static readonly string[] Messages = {
            "JUMP!",
            "RUN!",
            "WATCH OUT!",
            "LEVEL UP!",
            "WARNING: Danger ahead",
            "TIP: Try not to die",
            "OBJECTIVE: Survive",
            "HINT: The exit is somewhere",
            "ALERT: Enemy detected (3 seconds ago)",
            "CONGRATULATIONS! (on your previous action)",
        };

        private void Awake() {
            if (_player == null)
                _player = FindFirstObjectByType<PlayerController>();

            if (_config == null)
                Debug.LogWarning("[HelperSystem] No config assigned — using defaults.");

            // Subscribe to overload events
            _player.OnOverloadStarted.AddListener(OnOverloadStarted);
            _player.OnOverloadEnded.AddListener(OnOverloadEnded);

            ScheduleNextPopup();
        }

        private void OnDestroy() {
            if (_player == null) return;
            _player.OnOverloadStarted.RemoveListener(OnOverloadStarted);
            _player.OnOverloadEnded.RemoveListener(OnOverloadEnded);
        }

        private void Update() {
            if (_isMuted) return;

            if (Time.time >= _nextPopupTime)
                SpawnPopup();
        }

        private void SpawnPopup() {
            ScheduleNextPopup();

            // Pick a random misleading message
            string msg = Messages[Random.Range(0, Messages.Length)];
            OnMessageChanged.Invoke(msg);
            OnPopupSpawn.Invoke();

            // Add chaos — system interfering adds to overload meter
            AddInterferenceChaos(GetChaos(_config?.chaosOnPopupSpawn ?? 8f));
        }

        private void ScheduleNextPopup() {
            float min = _config?.minInterval ?? 3f;
            float max = _config?.maxInterval ?? 7f;
            _nextPopupTime = Time.time + Random.Range(min, max);
        }

        /// <summary>
        /// Call this when a popup visually covers the player.
        /// </summary>
        public void NotifyPlayerObstructed() {
            AddInterferenceChaos(GetChaos(_config?.chaosOnPlayerObstructed ?? 12f));
        }

        /// <summary>
        /// Call this when the player fails/takes damage due to popup interference.
        /// </summary>
        public void NotifyPlayerFailed() {
            AddInterferenceChaos(GetChaos(_config?.chaosOnPlayerFail ?? 20f));
        }

        private void AddInterferenceChaos(float amount) {
            _player?.AddChaos(amount, ChaosSource.SystemInterference);
        }

        private float GetChaos(float baseAmount) {
            float multiplier = _config?.aggressivenessMultiplier ?? 1f;
            return baseAmount * multiplier;
        }

        private void OnOverloadStarted() {
            _isMuted = true;
            OnSystemMuted.Invoke();
        }

        private void OnOverloadEnded() {
            _isMuted = false;
            ScheduleNextPopup(); // Reset timer after overload ends
            OnSystemRestored.Invoke();
        }

        /// <summary>
        /// Increase aggressiveness as the level progresses.
        /// Call from level manager when entering harder sections.
        /// </summary>
        public void SetAggressiveness(float multiplier) {
            if (_config != null)
                _config.aggressivenessMultiplier = multiplier;
        }
    }
}
