using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePromisedRun.UI {
    public class SceneLoadManager : MonoBehaviour {
        public static SceneLoadManager Instance { get; private set; }

        public System.Action<float> OnProgressChanged;
        public System.Action OnSceneLoaded;

        private AsyncOperation _currentLoadOperation;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Awaitable LoadSceneAsync(string sceneName) {
            await LoadSceneAsyncWithProgress(sceneName, false);
        }

        public async Awaitable LoadSceneAdditiveAsync(string sceneName) {
            await LoadSceneAsyncWithProgress(sceneName, true);
        }

        private async Awaitable LoadSceneAsyncWithProgress(string sceneName, bool additive) {
            Debug.Log($"[SceneLoadManager] Loading scene: {sceneName} (additive: {additive})");

            AsyncOperation asyncOp = additive
                ? SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
                : SceneManager.LoadSceneAsync(sceneName);

            if (asyncOp == null) {
                Debug.LogError($"[SceneLoadManager] Failed to start async load for scene: {sceneName}");
                return;
            }

            _currentLoadOperation = asyncOp;
            // WebGL is sensitive to manual scene-activation gating; keep default activation flow.
            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone) {
                // Normalize progress to [0..1] for UI (Unity reports up to 0.9 before final activation).
                float normalized = asyncOp.progress < 0.9f
                    ? Mathf.Clamp01(asyncOp.progress / 0.9f)
                    : 1f;
                OnProgressChanged?.Invoke(normalized);

                await Awaitable.NextFrameAsync();
            }

            OnSceneLoaded?.Invoke();
            OnProgressChanged?.Invoke(1f);

            Debug.Log($"[SceneLoadManager] Scene loaded: {sceneName}");
        }

        public float GetProgress() {
            return _currentLoadOperation?.progress ?? 0f;
        }

        public async Awaitable UnloadSceneAsync(string sceneName) {
            Debug.Log($"[SceneLoadManager] Unloading scene: {sceneName}");
            AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(sceneName);
            await asyncOp;
            Debug.Log($"[SceneLoadManager] Scene unloaded: {sceneName}");
        }

        public bool IsSceneLoaded(string sceneName) {
            return SceneManager.GetSceneByName(sceneName).isLoaded;
        }
    }
}
