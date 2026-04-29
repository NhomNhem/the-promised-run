using UnityEngine;

namespace ThePromisedRun.Audio {
    /// <summary>
    /// Simple AudioManager for Game Jam — BGM + SFX.
    /// Attach to a GameObject in each scene.
    /// </summary>
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioClip   _bgmClip;
        [SerializeField] [Range(0f,1f)] private float _bgmVolume = 0.4f;

        [Header("SFX — Popup")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip   _popupSFX;       // Windows XP error vibe
        [SerializeField] private AudioClip   _overloadSFX;    // Static burst
        [SerializeField] private AudioClip   _overloadSilence; // Short silence clip

        private void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_bgmSource == null) _bgmSource = gameObject.AddComponent<AudioSource>();
            if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();

            _bgmSource.loop        = true;
            _bgmSource.volume      = _bgmVolume;
            _bgmSource.playOnAwake = false;
            _sfxSource.playOnAwake = false;
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

        /// <summary>Called on Overload start — duck BGM, play static.</summary>
        public void OnOverloadStart() {
            PlayOverloadSFX();
            _bgmSource.volume = 0f;
        }

        /// <summary>Called on Overload end — restore BGM.</summary>
        public void OnOverloadEnd() {
            _bgmSource.volume = _bgmVolume;
        }
    }
}
