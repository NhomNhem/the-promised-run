using UnityEngine;
using OpenUtility.Data;

namespace ThePromisedRun.Audio {
    /// <summary>
    /// AudioManager — BGM + SFX for Scene_Manager (persistent, DontDestroyOnLoad).
    ///
    /// Decoupled from PlayerController via ScriptableVariable:
    ///   _overloadStateVar (ScriptableBool) — PlayerController writes, AudioManager reads.
    ///
    /// No FindFirstObjectByType, no direct PlayerController reference.
    /// Safe for multi-scene additive loading.
    /// </summary>
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioClip   _bgmClip;
        [SerializeField] [Range(0f,1f)] private float _bgmVolume = 0.4f;

        [Header("SFX")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip   _popupSFX;
        [SerializeField] private AudioClip   _overloadSFX;

        [Header("ScriptableVariables")]
        [SerializeField] private ScriptableBool _overloadStateVar; // PlayerController → AudioManager

        private void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_bgmSource == null) _bgmSource = gameObject.AddComponent<AudioSource>();
            if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();

            _bgmSource.loop        = true;
            _bgmSource.volume      = _bgmVolume;
            _bgmSource.playOnAwake = false;
            _sfxSource.playOnAwake = false;
        }

        private void OnEnable() {
            if (_overloadStateVar != null)
                _overloadStateVar.ValueChanged.AddListener(OnOverloadStateChanged);
        }

        private void OnDisable() {
            if (_overloadStateVar != null)
                _overloadStateVar.ValueChanged.RemoveListener(OnOverloadStateChanged);
        }

        private void Start() {
            if (_bgmClip != null) {
                _bgmSource.clip = _bgmClip;
                _bgmSource.Play();
            }
        }

        public void PlayPopupSFX() {
            if (_popupSFX != null) _sfxSource.PlayOneShot(_popupSFX, 0.7f);
        }

        public void PlayOverloadSFX() {
            if (_overloadSFX != null) _sfxSource.PlayOneShot(_overloadSFX, 1f);
        }

        public void SetBGMVolume(float vol) => _bgmSource.volume = vol;

        private void OnOverloadStateChanged(bool isOverloaded) {
            if (isOverloaded) OnOverloadStart();
            else              OnOverloadEnd();
        }

        private void OnOverloadStart() {
            PlayOverloadSFX();
            _bgmSource.volume = 0f;
        }

        private void OnOverloadEnd() {
            _bgmSource.volume = _bgmVolume;
        }
    }
}
