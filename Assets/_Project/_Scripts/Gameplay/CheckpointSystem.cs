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
            if (_player == null)       _player       = FindFirstObjectByType<PlayerController>();
            if (_playerHealth == null) _playerHealth = FindFirstObjectByType<Combat.PlayerHealth>();
        }

        private void Start() {
            // Save initial position as first checkpoint
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
            if (!_hasSave || _player == null) return;

            // Restore position
            _player.transform.position = _savedPosition;

            // Reset velocity
            if (_player.Rb != null)
                _player.Rb.linearVelocity = Vector3.zero;

            // Reset OL gauge
            _player.ResetChaos();

            // Restore HP
            if (_playerHealth != null)
                _playerHealth.RestoreHealth(_savedHP);

            OnCheckpointLoaded.Invoke();
            Debug.Log($"[Checkpoint] Respawned at {_savedPosition}");
        }
    }
}
