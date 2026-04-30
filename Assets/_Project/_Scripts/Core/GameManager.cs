using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePromisedRun.Core {
    /// <summary>
    /// GameManager — coordinates Player spawn and level transitions.
    /// Lives in Scene_Manager (DontDestroyOnLoad).
    ///
    /// Spawn flow:
    ///   1. Scene_GamePlay loads → Player prefab instantiated at (0,0,0)
    ///   2. Level scene loads additively
    ///   3. GameManager.OnSceneLoaded fires → finds SpawnPoint in level → moves Player there
    ///
    /// SpawnPoint: any GameObject tagged "SpawnPoint" in the level scene.
    /// If multiple SpawnPoints exist, uses the first one found.
    /// </summary>
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }

        [Header("Player")]
        [SerializeField] private GameObject _playerPrefab;

        [Header("Config")]
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private string _spawnPointTag = "SpawnPoint";

        private GameObject _playerInstance;

        private void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            // Only react to level scenes (additive loads that contain a SpawnPoint)
            if (mode != LoadSceneMode.Additive) return;

            var spawnPoint = FindSpawnPointInScene(scene);
            if (spawnPoint == null) return;

            // Find or spawn Player
            if (_playerInstance == null)
                _playerInstance = GameObject.FindWithTag(_playerTag);

            if (_playerInstance == null && _playerPrefab != null) {
                _playerInstance = Instantiate(_playerPrefab);
                Debug.Log($"[GameManager] Spawned Player from prefab at {spawnPoint.position}");
            }

            if (_playerInstance != null) {
                MovePlayerToSpawnPoint(spawnPoint);
            } else {
                Debug.LogWarning("[GameManager] Player not found and no prefab assigned. " +
                                 "Assign _playerPrefab in Inspector or ensure Player is in Scene_GamePlay.");
            }
        }

        private Transform FindSpawnPointInScene(Scene scene) {
            foreach (var root in scene.GetRootGameObjects()) {
                // Check root itself
                if (root.CompareTag(_spawnPointTag))
                    return root.transform;

                // Check children
                var found = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in found) {
                    if (t.CompareTag(_spawnPointTag))
                        return t;
                }
            }
            return null;
        }

        private void MovePlayerToSpawnPoint(Transform spawnPoint) {
            // Disable physics briefly to prevent collision issues during teleport
            var rb = _playerInstance.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            _playerInstance.transform.position = spawnPoint.position;
            _playerInstance.transform.rotation = spawnPoint.rotation;

            Debug.Log($"[GameManager] Player moved to SpawnPoint at {spawnPoint.position}");
        }

        /// <summary>
        /// Called by CheckpointSystem to respawn Player at a saved position.
        /// </summary>
        public void RespawnPlayerAt(Vector3 position) {
            if (_playerInstance == null)
                _playerInstance = GameObject.FindWithTag(_playerTag);

            if (_playerInstance == null) {
                Debug.LogWarning("[GameManager] Cannot respawn — Player not found.");
                return;
            }

            var rb = _playerInstance.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            _playerInstance.transform.position = position;
            Debug.Log($"[GameManager] Player respawned at {position}");
        }

        /// <summary>Returns the current Player instance (may be null before spawn).</summary>
        public GameObject GetPlayer() => _playerInstance;
    }
}
