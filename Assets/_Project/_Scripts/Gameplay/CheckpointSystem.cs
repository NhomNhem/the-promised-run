using UnityEngine;
using UnityEngine.Events;

namespace ThePromisedRun.Gameplay {
    /// <summary>
    /// Checkpoint system — GDD §9.3.
    /// Auto-saves every 45s. Stores position, HP, OL gauge.
    /// Respawn: fade in at checkpoint, OL reset to 0.
    /// </summary>
    public class CheckpointSystem : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float _autoSaveInterval = 45f;

        [Header("References")]
        [SerializeField] private PlayerController _player;
        [SerializeField] private Combat.PlayerHealth _playerHealth;

        [Header("Events")]
        public UnityEvent OnCheckpointSaved  = new UnityEvent();
        public UnityEvent OnCheckpointLoaded = new UnityEvent();

        // Saved state
        private Vector3 _savedPosition;
        private float   _savedHP;
        private bool    _hasSave;
        private float   _autoSaveTimer;

        private void Awake() {
            // _player can be assigned in Inspector or found at runtime (cross-scene additive).
            // _playerHealth lives in Scene_GamePlay — lazy-find at Start() after all scenes load.
            if (_player == null)
                Debug.LogWarning("[CheckpointSystem] PlayerController not assigned — will search at Start().");
        }

        private void Start() {
            // Lazy-find cross-scene references (Player is in Scene_GamePlay, loaded additively)
            if (_player == null)
                _player = FindFirstObjectByType<PlayerController>();
            if (_playerHealth == null)
                _playerHealth = FindFirstObjectByType<Combat.PlayerHealth>();

            if (_player == null)
                Debug.LogWarning("[CheckpointSystem] PlayerController not found. Respawn will not work.");

            SaveCheckpoint();
        }

        private void Update() {
            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= _autoSaveInterval) {
                _autoSaveTimer = 0f;
                SaveCheckpoint();
            }
        }

        public void SaveCheckpoint() {
            if (_player == null) return;
            _savedPosition = _player.transform.position;
            _savedHP       = _playerHealth != null ? _playerHealth.Health : 100f;
            _hasSave       = true;
            OnCheckpointSaved.Invoke();
            Debug.Log($"[Checkpoint] Saved at {_savedPosition}");
        }

        public void Respawn() {
            if (!_hasSave) return;

            // Use GameManager for cross-scene respawn if available
            if (Core.GameManager.Instance != null) {
                Core.GameManager.Instance.RespawnPlayerAt(_savedPosition);
            } else if (_player != null) {
                _player.transform.position = _savedPosition;
                if (_player.Rb != null) _player.Rb.linearVelocity = Vector3.zero;
            } else {
                Debug.LogWarning("[CheckpointSystem] Cannot respawn — no Player reference.");
                return;
            }

            // Reset OL gauge
            if (_player != null) _player.ResetChaos();
            else {
                var pc = FindFirstObjectByType<PlayerController>();
                pc?.ResetChaos();
            }

            // Restore HP
            if (_playerHealth != null)
                _playerHealth.RestoreHealth(_savedHP);
            else {
                var ph = FindFirstObjectByType<Combat.PlayerHealth>();
                ph?.RestoreHealth(_savedHP);
            }

            OnCheckpointLoaded.Invoke();
            Debug.Log($"[Checkpoint] Respawned at {_savedPosition}");
        }
    }
}
