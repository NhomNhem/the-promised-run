using UnityEngine;
using UnityEngine.SceneManagement;
using Ami.BroAudio;

namespace ThePromisedRun.Audio
{
    /// <summary>
    /// AudioManager — Singleton (DontDestroyOnLoad) that manages BGM and SFX via BroAudio.
    /// Persists across scenes in Scene_Manager. Saves/loads settings via PlayerPrefs.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Fields

        [Header("Settings")]
        [SerializeField] private AudioSettingsSO _settings;

        [Header("BGM")]
        [SerializeField] private SoundID _menuBgmId;
        [SerializeField] private SoundID _gameplayBgmId;

        [Header("SFX")]
        [SerializeField] private SoundID _sfxHit;
        [SerializeField] private SoundID _sfxDash;
        [SerializeField] private SoundID _sfxJump;
        [SerializeField] private SoundID _sfxAttack;
        [SerializeField] private SoundID _sfxDeath;

        public static AudioManager Instance { get; private set; }

        /// <summary>Exposes the AudioSettingsSO for read access (e.g. SettingsPanelController snapshot).</summary>
        public AudioSettingsSO Settings => _settings;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            ApplyAllVolumes();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnApplicationQuit() => SaveSettings();

        #endregion

        #region Settings — Load / Save

        /// <summary>
        /// Reads volume and quality settings from PlayerPrefs (defaults to 1.0f / quality 1).
        /// Applies QualitySettings immediately.
        /// </summary>
        private void LoadSettings()
        {
            if (_settings == null)
            {
                Debug.LogWarning("[AudioManager] AudioSettingsSO not assigned — using runtime defaults.");
                return;
            }

            _settings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            _settings.musicVolume  = PlayerPrefs.GetFloat("MusicVolume",  1f);
            _settings.sfxVolume    = PlayerPrefs.GetFloat("SfxVolume",    1f);

            int quality = PlayerPrefs.GetInt("GraphicsQuality", 1);
            QualitySettings.SetQualityLevel(quality, true);
        }

        /// <summary>
        /// Writes current volume and quality settings to PlayerPrefs and flushes to disk.
        /// </summary>
        public void SaveSettings()
        {
            if (_settings == null) return;

            PlayerPrefs.SetFloat("MasterVolume", _settings.masterVolume);
            PlayerPrefs.SetFloat("MusicVolume",  _settings.musicVolume);
            PlayerPrefs.SetFloat("SfxVolume",    _settings.sfxVolume);
            PlayerPrefs.SetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
            PlayerPrefs.Save();
        }

        #endregion

        #region Volume

        /// <summary>
        /// Applies all three volume channels to BroAudio.
        /// BroAudio range is 0–10; slider range is 0–1, so we multiply by 10.
        /// </summary>
        public void ApplyAllVolumes()
        {
            if (_settings == null) return;

            BroAudio.SetVolume(BroAudioType.All,     _settings.masterVolume * 10f);
            BroAudio.SetVolume(BroAudioType.Music,   _settings.musicVolume  * 10f);
            BroAudio.SetVolume(BroAudioType.SFX,     _settings.sfxVolume    * 10f);
        }

        /// <summary>Sets master volume (0–1), updates SO, and applies to BroAudio.</summary>
        public void SetMasterVolume(float vol)
        {
            if (_settings == null) return;
            vol = Mathf.Clamp01(vol);
            _settings.masterVolume = vol;
            BroAudio.SetVolume(BroAudioType.All, vol * 10f);
        }

        /// <summary>Sets music volume (0–1), updates SO, and applies to BroAudio.</summary>
        public void SetMusicVolume(float vol)
        {
            if (_settings == null) return;
            vol = Mathf.Clamp01(vol);
            _settings.musicVolume = vol;
            BroAudio.SetVolume(BroAudioType.Music, vol * 10f);
        }

        /// <summary>Sets SFX volume (0–1), updates SO, and applies to BroAudio.</summary>
        public void SetSfxVolume(float vol)
        {
            if (_settings == null) return;
            vol = Mathf.Clamp01(vol);
            _settings.sfxVolume = vol;
            BroAudio.SetVolume(BroAudioType.SFX, vol * 10f);
        }

        #endregion

        #region BGM

        /// <summary>Plays the main-menu BGM track.</summary>
        public void PlayMenuBGM()
        {
            if (!_menuBgmId.IsValid())
            {
                Debug.LogWarning("[AudioManager] menuBgmId not assigned.");
                return;
            }
            BroAudio.Play(_menuBgmId);
        }

        /// <summary>Crossfades to the gameplay BGM track (stops current music with 1 s fade).</summary>
        public void PlayGameplayBGM()
        {
            if (!_gameplayBgmId.IsValid())
            {
                Debug.LogWarning("[AudioManager] gameplayBgmId not assigned.");
                return;
            }
            BroAudio.Stop(BroAudioType.Music, 1f);
            BroAudio.Play(_gameplayBgmId);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Scene_MainMenu")   PlayMenuBGM();
            else if (scene.name == "Scene_GamePlay") PlayGameplayBGM();
        }

        #endregion

        #region SFX

        public void PlayHit()    => PlaySfx(_sfxHit,    "Hit");
        public void PlayDash()   => PlaySfx(_sfxDash,   "Dash");
        public void PlayJump()   => PlaySfx(_sfxJump,   "Jump");
        public void PlayAttack() => PlaySfx(_sfxAttack, "Attack");
        public void PlayDeath()  => PlaySfx(_sfxDeath,  "Death");

        private void PlaySfx(SoundID id, string sfxName)
        {
            if (!id.IsValid())
            {
                Debug.LogWarning($"[AudioManager] SFX '{sfxName}' not assigned.");
                return;
            }
            BroAudio.Play(id);
        }

        #endregion
    }
}
