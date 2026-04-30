using UnityEngine;
using UnityEngine.Events;

namespace ThePromisedRun.Gameplay.Level {
    /// <summary>
    /// Level exit trigger — GDD §9.1.
    /// Player walks into trigger zone → level complete → load next scene.
    /// Uses SceneLoadManager for proper multi-scene additive loading.
    /// </summary>
    public class LevelExitTrigger : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private string _nextLevelSceneName = "";
        [SerializeField] private float  _transitionDelay    = 1.0f;

        [Header("Visual")]
        [SerializeField] private GameObject _exitVisual;

        [Header("Events")]
        public UnityEvent OnPlayerReachedExit = new UnityEvent();

        private bool _triggered;

        private void OnTriggerEnter(Collider other) {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            OnPlayerReachedExit.Invoke();
            Debug.Log($"[LevelExit] Player reached exit! Next: {_nextLevelSceneName}");

            if (!string.IsNullOrEmpty(_nextLevelSceneName))
                Invoke(nameof(LoadNextLevel), _transitionDelay);
            else
                Debug.LogWarning("[LevelExit] _nextLevelSceneName is empty — assign it in Inspector.");
        }

        private async void LoadNextLevel() {
            var slm = UI.SceneLoadManager.Instance;
            if (slm == null) {
                Debug.LogError("[LevelExit] SceneLoadManager.Instance is null.");
                return;
            }

            // Get current level scene name to unload it
            string currentLevel = gameObject.scene.name;

            // Load next level additively
            await slm.LoadSceneAdditiveAsync(_nextLevelSceneName);

            // Set new level as active scene
            var nextScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(_nextLevelSceneName);
            if (nextScene.IsValid())
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(nextScene);

            // Unload current level
            await slm.UnloadSceneAsync(currentLevel);
        }

        private void OnDrawGizmos() {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            var col = GetComponent<BoxCollider>();
            if (col != null)
                Gizmos.DrawCube(transform.position + col.center, col.size);
        }
    }
}
