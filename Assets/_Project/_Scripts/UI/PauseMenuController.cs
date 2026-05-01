using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using DG.Tweening;

namespace ThePromisedRun.UI
{
    /// <summary>
    /// PauseMenuController — manages the Pause Menu overlay.
    /// Receives its VisualElement root from HUDManager via Initialize().
    /// Handles ESC input, Time.timeScale, and scene transitions.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Scene Names")]
        [SerializeField] private string _mainMenuSceneName = "Scene_MainMenu";
        [SerializeField] private string _gameplaySceneName = "Scene_GamePlay";
        [SerializeField] private string _hudSceneName      = "Scene_HUD";

        [Header("Fade")]
        [SerializeField] private float _fadeInDuration  = 0.2f;
        [SerializeField] private float _fadeOutDuration = 0.15f;

        #endregion

        #region Private Fields

        private VisualElement _pauseLayer;
        private VisualElement _pauseOverlay;
        private Button        _btnResume;
        private Button        _btnTestEnding;
        private Button        _btnSettings;
        private Button        _btnQuit;

        private SettingsPanelController _settingsPanel;
        private Tween _fadeTween;
        private bool  _isPaused;
        private bool  _isTransitioning;
        private bool  _initialized;
        private bool  _settingsOpenFromEsc;

        #endregion

        #region Initialize

        /// <summary>
        /// Called by HUDManager with the shared UIDocument root.
        /// Queries pause elements and wires button callbacks.
        /// </summary>
        public void Initialize(VisualElement root)
        {
            if (root == null)
            {
                Debug.LogWarning("[PauseMenuController] Initialize called with null root.");
                return;
            }

            _pauseLayer   = root.Q<VisualElement>("pause-layer");
            _pauseOverlay = root.Q<VisualElement>("pause-overlay");
            _btnResume    = root.Q<Button>("btn-resume");
            _btnTestEnding = root.Q<Button>("btn-test-ending");
            _btnSettings  = root.Q<Button>("btn-settings");
            _btnQuit      = root.Q<Button>("btn-quit");

            if (_pauseLayer == null)
            {
                Debug.LogError("[PauseMenuController] 'pause-layer' not found in UXML.");
                return;
            }

            // Ensure hidden on start
            _pauseLayer.AddToClassList("hidden");

            // Wire button callbacks
            _btnResume?.RegisterCallback<ClickEvent>(_   => OnResumeClicked());
            _btnTestEnding?.RegisterCallback<ClickEvent>(_ => OnTestEndingClicked());
            _btnSettings?.RegisterCallback<ClickEvent>(_ => OnSettingsClicked());
            _btnQuit?.RegisterCallback<ClickEvent>(_     => OnQuitClicked());

            // Get SettingsPanelController from same GameObject
            _settingsPanel = GetComponent<SettingsPanelController>();

            _initialized = true;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!_initialized) return;
            if (_isTransitioning) return;

            // Only respond to ESC when NOT in MainMenu
            if (SceneManager.GetActiveScene().name == _mainMenuSceneName) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
            // Ensure timeScale is restored if destroyed while paused
            if (_isPaused)
                Time.timeScale = 1f;
        }

        #endregion

        #region Pause Logic

        private void TogglePause()
        {
            if (_isPaused)
            {
                if (_settingsOpenFromEsc)
                {
                    // ESC lần 2: đóng Settings đang mở từ ESC (không apply)
                    _settingsPanel?.Cancel();
                }
                else
                {
                    CloseAsync();
                }
            }
            else
            {
                OpenSettingsFromEsc();
            }
        }

        /// <summary>
        /// Mở Settings Panel trực tiếp từ ESC — pause game và track trạng thái.
        /// </summary>
        public void OpenSettingsFromEsc()
        {
            if (_settingsPanel == null) return;
            _isPaused = true;
            _settingsOpenFromEsc = true;
            Time.timeScale = 0f;
            _settingsPanel.OpenFromEsc(this);
        }

        /// <summary>
        /// Callback từ SettingsPanelController khi đóng panel mở từ ESC.
        /// </summary>
        public void OnSettingsClosedFromEsc()
        {
            _settingsOpenFromEsc = false;
            _isPaused = false;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Opens the pause menu: freezes time, shows layer, fades in overlay.
        /// </summary>
        private void OpenAsync()
        {
            if (_pauseLayer == null) return;

            _isPaused = true;
            Time.timeScale = 0f;

            // Show layer (remove hidden class)
            _pauseLayer.RemoveFromClassList("hidden");

            // Reset overlay opacity and fade in
            if (_pauseOverlay != null)
            {
                _pauseOverlay.style.opacity = 0f;

                _fadeTween?.Kill();
                _fadeTween = DOTween.To(
                    () => _pauseOverlay.style.opacity.value,
                    x  => _pauseOverlay.style.opacity = x,
                    1f, _fadeInDuration
                ).SetEase(Ease.OutCubic)
                 .SetUpdate(true);
            }
        }

        /// <summary>
        /// Closes the pause menu: fades out overlay, hides layer, restores time.
        /// </summary>
        private void CloseAsync()
        {
            if (_pauseLayer == null) return;

            _fadeTween?.Kill();

            if (_pauseOverlay != null)
            {
                _fadeTween = DOTween.To(
                    () => _pauseOverlay.style.opacity.value,
                    x  => _pauseOverlay.style.opacity = x,
                    0f, _fadeOutDuration
                ).SetEase(Ease.InCubic)
                 .SetUpdate(true)
                 .OnComplete(() =>
                 {
                     if (_pauseLayer != null)
                         _pauseLayer.AddToClassList("hidden");
                     _isPaused = false;
                     Time.timeScale = 1f;
                 });
            }
            else
            {
                _pauseLayer.AddToClassList("hidden");
                _isPaused = false;
                Time.timeScale = 1f;
            }
        }

        #endregion

        #region Button Handlers

        private void OnResumeClicked()
        {
            CloseAsync();
        }

        private void OnSettingsClicked()
        {
            _settingsPanel?.Open();
        }

        private void OnTestEndingClicked()
        {
            // Ensure gameplay resumes so ending coroutine uses normal time scale.
            _fadeTween?.Kill();
            if (_pauseLayer != null)
                _pauseLayer.AddToClassList("hidden");
            _isPaused = false;
            Time.timeScale = 1f;

            var ending = FindFirstObjectByType<EndingSequenceController>();
            if (ending != null)
                ending.PlayEnding();
            else
                Debug.LogWarning("[PauseMenuController] EndingSequenceController not found for test ending.");
        }

        private void OnQuitClicked()
        {
            _isTransitioning = true;
            Time.timeScale = 1f;
            _isPaused = false;

            // Hide pause layer immediately
            _fadeTween?.Kill();
            if (_pauseLayer != null)
                _pauseLayer.AddToClassList("hidden");

            _ = QuitToMainMenuAsync();
        }

        private async Awaitable QuitToMainMenuAsync()
        {
            if (SceneLoadManager.Instance == null)
            {
                Debug.LogError("[PauseMenuController] SceneLoadManager.Instance is null.");
                _isTransitioning = false;
                return;
            }

            // Unload gameplay scenes
            if (SceneLoadManager.Instance.IsSceneLoaded(_gameplaySceneName))
                await SceneLoadManager.Instance.UnloadSceneAsync(_gameplaySceneName);

            // Load main menu
            await SceneLoadManager.Instance.LoadSceneAdditiveAsync(_mainMenuSceneName);

            // Unload HUD last (this scene will be destroyed)
            if (SceneLoadManager.Instance.IsSceneLoaded(_hudSceneName))
                await SceneLoadManager.Instance.UnloadSceneAsync(_hudSceneName);

            _isTransitioning = false;
        }

        #endregion
    }
}
