using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePromisedRun.Core {
    /// <summary>
    /// GameBootstrapper — ensures Scene_Manager is always loaded first,
    /// regardless of which scene the developer presses Play from in the Editor.
    ///
    /// In a real build, Scene_MainMenu (index 0) is always the entry point.
    /// In the Editor, developers often press Play from a level scene for quick iteration.
    /// This bootstrapper detects that case and loads Scene_Manager additively so that
    /// SceneLoadManager, AudioManager, and other persistent systems are always present.
    ///
    /// Uses [RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] — runs before any Awake().
    /// Editor-only behavior: in builds, Scene_MainMenu handles this naturally.
    /// </summary>
    public static class GameBootstrapper {

        private const int ManagerBuildIndex  = 1; // Scene_Manager
        private const int MainMenuBuildIndex = 0; // Scene_MainMenu

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() {
            // Always ensure Scene_Manager is loaded (contains SceneLoadManager + AudioManager)
            var managerScene = SceneManager.GetSceneByBuildIndex(ManagerBuildIndex);
            if (!managerScene.isLoaded) {
                Debug.Log("[GameBootstrapper] Loading Scene_Manager additively...");
                SceneManager.LoadScene(ManagerBuildIndex, LoadSceneMode.Additive);
            }

#if UNITY_EDITOR
            // In Editor: if we're NOT starting from Scene_MainMenu, log a hint
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex != MainMenuBuildIndex) {
                Debug.Log($"[GameBootstrapper] Playing from '{activeScene.name}' (not MainMenu). " +
                          $"Scene_Manager loaded additively. " +
                          $"For full flow, open Scene_MainMenu and press Play.");
            }
#endif
        }
    }
}
