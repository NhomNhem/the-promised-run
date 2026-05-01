using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using ThePromisedRun.Audio;

namespace ThePromisedRun.UI
{
    /// <summary>
    /// SettingsPanelController — manages the Settings overlay (audio sliders + graphics quality).
    /// Receives its VisualElement root from HUDManager or MainMenuController via Initialize().
    /// Clones Settings.uxml into the shared root and wires all controls.
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI Asset")]
        [SerializeField] private VisualTreeAsset _settingsUxml;

        [Header("Fade")]
        [SerializeField] private float _fadeInDuration  = 0.2f;
        [SerializeField] private float _fadeOutDuration = 0.15f;

        #endregion

        #region Private Fields

        private VisualElement _overlay;
        private Slider        _sliderMaster;
        private Slider        _sliderMusic;
        private Slider        _sliderSfx;
        private Button        _btnQualityLow;
        private Button        _btnQualityMedium;
        private Button        _btnQualityHigh;
        private Button        _btnApply;
        private Button        _btnCancel;
        private Button        _btnReset;
        private Button        _btnBack;
        private Button        _btnEndGame;

        // Snapshot for Cancel
        private float _snapMaster;
        private float _snapMusic;
        private float _snapSfx;
        private int   _snapQuality;

        private Tween _fadeTween;
        private bool  _initialized;

        // ESC context tracking
        private bool                 _openedFromEsc;
        private PauseMenuController  _pauseMenuRef;

        #endregion

        #region Initialize

        /// <summary>
        /// Called by HUDManager or MainMenuController with the shared UIDocument root.
        /// Clones Settings.uxml into root, queries all elements, wires callbacks.
        /// </summary>
        public void Initialize(VisualElement root)
        {
            if (root == null)
            {
                Debug.LogWarning("[SettingsPanelController] Initialize called with null root.");
                return;
            }

            if (_settingsUxml == null)
            {
                Debug.LogError("[SettingsPanelController] _settingsUxml not assigned in Inspector.");
                return;
            }

            // Clone Settings.uxml into the shared root
            _settingsUxml.CloneTree(root);

            // Query elements
            _overlay          = root.Q<VisualElement>("settings-overlay");
            _sliderMaster     = root.Q<Slider>("slider-master");
            _sliderMusic      = root.Q<Slider>("slider-music");
            _sliderSfx        = root.Q<Slider>("slider-sfx");
            _btnQualityLow    = root.Q<Button>("btn-quality-low");
            _btnQualityMedium = root.Q<Button>("btn-quality-medium");
            _btnQualityHigh   = root.Q<Button>("btn-quality-high");
            _btnApply         = root.Q<Button>("btn-apply");
            _btnCancel        = root.Q<Button>("btn-cancel");
            _btnReset         = root.Q<Button>("btn-reset");
            _btnBack          = root.Q<Button>("btn-back");
            _btnEndGame       = root.Q<Button>("btn-endgame");

            if (_overlay == null)
            {
                Debug.LogError("[SettingsPanelController] 'settings-overlay' not found in UXML.");
                return;
            }

            // Hide initially
            _overlay.style.display = DisplayStyle.None;
            _overlay.style.opacity = 0f;

            // Wire slider callbacks — preview audio in realtime
            _sliderMaster?.RegisterValueChangedCallback(evt =>
                AudioManager.Instance?.SetMasterVolume(evt.newValue));
            _sliderMusic?.RegisterValueChangedCallback(evt =>
                AudioManager.Instance?.SetMusicVolume(evt.newValue));
            _sliderSfx?.RegisterValueChangedCallback(evt =>
                AudioManager.Instance?.SetSfxVolume(evt.newValue));

            // Wire quality buttons
            _btnQualityLow?.RegisterCallback<ClickEvent>(_    => SetQuality(0));
            _btnQualityMedium?.RegisterCallback<ClickEvent>(_ => SetQuality(1));
            _btnQualityHigh?.RegisterCallback<ClickEvent>(_   => SetQuality(2));

            // Wire action buttons
            _btnApply?.RegisterCallback<ClickEvent>(_  => Apply());
            _btnCancel?.RegisterCallback<ClickEvent>(_ => Cancel());
            _btnReset?.RegisterCallback<ClickEvent>(_  => Reset());
            _btnBack?.RegisterCallback<ClickEvent>(_   => OnBackClicked());
            _btnEndGame?.RegisterCallback<ClickEvent>(_ => OnEndGameClicked());

            _initialized = true;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Mở Settings từ ESC trong gameplay — hiện btn-back, ẩn btn-cancel, pause game.
        /// </summary>
        public void OpenFromEsc(PauseMenuController pauseMenu)
        {
            _openedFromEsc = true;
            _pauseMenuRef  = pauseMenu;

            if (_btnBack   != null) _btnBack.style.display   = DisplayStyle.Flex;
            if (_btnCancel != null) _btnCancel.style.display = DisplayStyle.None;

            Open();
        }

        /// <summary>
        /// Opens the Settings panel: snapshots current values, populates controls, fades in.
        /// </summary>
        public void Open()
        {
            if (!_initialized || _overlay == null) return;

            // Default context (Main Menu / Pause button): hide Back, show Cancel
            if (!_openedFromEsc)
            {
                if (_btnBack != null) _btnBack.style.display = DisplayStyle.None;
                if (_btnCancel != null) _btnCancel.style.display = DisplayStyle.Flex;
            }

            // Snapshot current in-memory values for Cancel
            AudioSettingsSO settings = AudioManager.Instance?.Settings;
            _snapMaster  = settings != null ? settings.masterVolume : PlayerPrefs.GetFloat("MasterVolume", 1f);
            _snapMusic   = settings != null ? settings.musicVolume  : PlayerPrefs.GetFloat("MusicVolume",  1f);
            _snapSfx     = settings != null ? settings.sfxVolume    : PlayerPrefs.GetFloat("SfxVolume",    1f);
            _snapQuality = QualitySettings.GetQualityLevel();

            // Populate sliders from current values (without triggering callbacks)
            if (_sliderMaster != null) _sliderMaster.SetValueWithoutNotify(_snapMaster);
            if (_sliderMusic  != null) _sliderMusic.SetValueWithoutNotify(_snapMusic);
            if (_sliderSfx    != null) _sliderSfx.SetValueWithoutNotify(_snapSfx);

            // Update quality button active state
            UpdateQualityButtons(_snapQuality);

            // Show and fade in
            _overlay.style.display = DisplayStyle.Flex;
            _overlay.style.opacity = 0f;

            _fadeTween?.Kill();
            _fadeTween = DOTween.To(
                () => _overlay.style.opacity.value,
                x  => _overlay.style.opacity = x,
                1f, _fadeInDuration
            ).SetEase(Ease.OutCubic)
             .SetUpdate(true);
        }

        /// <summary>Saves settings and closes the panel.</summary>
        public void Apply()
        {
            AudioManager.Instance?.SaveSettings();
            Close();
        }

        /// <summary>Restores snapshot values and closes the panel.</summary>
        public void Cancel()
        {
            // Restore audio volumes to what they were when the panel opened
            AudioManager.Instance?.SetMasterVolume(_snapMaster);
            AudioManager.Instance?.SetMusicVolume(_snapMusic);
            AudioManager.Instance?.SetSfxVolume(_snapSfx);

            // Restore quality
            QualitySettings.SetQualityLevel(_snapQuality, true);

            // Restore slider visuals without triggering callbacks
            if (_sliderMaster != null) _sliderMaster.SetValueWithoutNotify(_snapMaster);
            if (_sliderMusic  != null) _sliderMusic.SetValueWithoutNotify(_snapMusic);
            if (_sliderSfx    != null) _sliderSfx.SetValueWithoutNotify(_snapSfx);
            UpdateQualityButtons(_snapQuality);

            // Nếu mở từ ESC, notify PauseMenuController để resume game
            if (_openedFromEsc)
            {
                RestoreEscContext();
            }

            Close();
        }

        /// <summary>
        /// Nút "QUAY LẠI" — lưu settings và đóng panel, resume game.
        /// Chỉ hiển thị khi Settings được mở từ ESC.
        /// </summary>
        private void OnBackClicked()
        {
            Apply(); // lưu settings trước khi đóng
            RestoreEscContext();
            // Close() đã được gọi bên trong Apply() → Close()
        }

        private void OnEndGameClicked()
        {
            // Ensure settings closes and gameplay resumes before starting ending sequence.
            if (_openedFromEsc)
                RestoreEscContext();

            Time.timeScale = 1f;
            Close();

            var ending = FindFirstObjectByType<EndingSequenceController>();
            if (ending != null)
                ending.PlayEnding();
            else
                Debug.LogWarning("[SettingsPanelController] EndingSequenceController not found. End game test skipped.");
        }

        /// <summary>Reset ESC context và notify PauseMenuController.</summary>
        private void RestoreEscContext()
        {
            _openedFromEsc = false;
            _pauseMenuRef?.OnSettingsClosedFromEsc();
            _pauseMenuRef = null;

            if (_btnBack   != null) _btnBack.style.display   = DisplayStyle.None;
            if (_btnCancel != null) _btnCancel.style.display = DisplayStyle.Flex;
        }

        /// <summary>Resets all settings to defaults and applies immediately (does NOT save).</summary>
        public void Reset()
        {
            const float DefaultVolume  = 1f;
            const int   DefaultQuality = 1;

            AudioManager.Instance?.SetMasterVolume(DefaultVolume);
            AudioManager.Instance?.SetMusicVolume(DefaultVolume);
            AudioManager.Instance?.SetSfxVolume(DefaultVolume);
            QualitySettings.SetQualityLevel(DefaultQuality, true);

            if (_sliderMaster != null) _sliderMaster.SetValueWithoutNotify(DefaultVolume);
            if (_sliderMusic  != null) _sliderMusic.SetValueWithoutNotify(DefaultVolume);
            if (_sliderSfx    != null) _sliderSfx.SetValueWithoutNotify(DefaultVolume);
            UpdateQualityButtons(DefaultQuality);
        }

        /// <summary>Fades out and hides the Settings overlay.</summary>
        public void Close()
        {
            if (_overlay == null) return;

            _fadeTween?.Kill();
            _fadeTween = DOTween.To(
                () => _overlay.style.opacity.value,
                x  => _overlay.style.opacity = x,
                0f, _fadeOutDuration
            ).SetEase(Ease.InCubic)
             .SetUpdate(true)
             .OnComplete(() =>
             {
                 if (_overlay != null)
                     _overlay.style.display = DisplayStyle.None;
             });
        }

        #endregion

        #region Private Helpers

        private void SetQuality(int level)
        {
            QualitySettings.SetQualityLevel(level, true);
            UpdateQualityButtons(level);
        }

        private void UpdateQualityButtons(int activeLevel)
        {
            SetQualityButtonActive(_btnQualityLow,    activeLevel == 0);
            SetQualityButtonActive(_btnQualityMedium, activeLevel == 1);
            SetQualityButtonActive(_btnQualityHigh,   activeLevel == 2);
        }

        private void SetQualityButtonActive(Button btn, bool active)
        {
            if (btn == null) return;
            if (active) btn.AddToClassList("active");
            else        btn.RemoveFromClassList("active");
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            _fadeTween?.Kill();
        }

        #endregion
    }
}
