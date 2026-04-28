using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace _Project._Scripts.UI {
    public class SettingsPanel : MonoBehaviour {
        [Header("Volume")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeLabel;

        [Header("Brightness")]
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private Text brightnessLabel;

        [Header("Graphics")]
        [SerializeField] private Button lowQualityButton;
        [SerializeField] private Button mediumQualityButton;
        [SerializeField] private Button highQualityButton;

        [Header("Actions")]
        [SerializeField] private Button closeButton;

        // Follow naming rule for private instance fields
        private float _currentBrightness = 50f;

        // Cached UnityAction delegates so we can remove them later
        private UnityAction<float> _onVolumeChangedAction;
        private UnityAction<float> _onBrightnessChangedAction;
        private UnityAction _onLowQualityClicked;
        private UnityAction _onMediumQualityClicked;
        private UnityAction _onHighQualityClicked;
        private UnityAction _onCloseClicked;

        private void Awake() {
            // Create cached delegates
            _onVolumeChangedAction = OnVolumeChanged;
            _onBrightnessChangedAction = OnBrightnessChanged;
            _onLowQualityClicked = () => SetGraphicsQuality(0);
            _onMediumQualityClicked = () => SetGraphicsQuality(1);
            _onHighQualityClicked = () => SetGraphicsQuality(2);
            _onCloseClicked = OnCloseClicked;
        }

        private void OnEnable() {
            // Ensure sliders exist before wiring
            if (volumeSlider != null) {
                // Assume slider range is 0..100 for UI; reflect current AudioListener volume
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 100f;
                volumeSlider.value = AudioListener.volume * 100f;
                volumeSlider.onValueChanged.AddListener(_onVolumeChangedAction);
                UpdateVolumeLabel(volumeSlider.value);
            }

            if (brightnessSlider != null) {
                brightnessSlider.minValue = 0f;
                brightnessSlider.maxValue = 100f;
                brightnessSlider.value = _currentBrightness;
                brightnessSlider.onValueChanged.AddListener(_onBrightnessChangedAction);
                UpdateBrightnessLabel(brightnessSlider.value);
            }

            if (lowQualityButton != null) lowQualityButton.onClick.AddListener(_onLowQualityClicked);
            if (mediumQualityButton != null) mediumQualityButton.onClick.AddListener(_onMediumQualityClicked);
            if (highQualityButton != null) highQualityButton.onClick.AddListener(_onHighQualityClicked);
            if (closeButton != null) closeButton.onClick.AddListener(_onCloseClicked);

            // Reflect current quality in UI
            UpdateQualityButtonStates(QualitySettings.GetQualityLevel());
        }

        private void OnDisable() {
            if (volumeSlider != null) {
                volumeSlider.onValueChanged.RemoveListener(_onVolumeChangedAction);
            }
            if (brightnessSlider != null) {
                brightnessSlider.onValueChanged.RemoveListener(_onBrightnessChangedAction);
            }

            if (lowQualityButton != null) lowQualityButton.onClick.RemoveListener(_onLowQualityClicked);
            if (mediumQualityButton != null) mediumQualityButton.onClick.RemoveListener(_onMediumQualityClicked);
            if (highQualityButton != null) highQualityButton.onClick.RemoveListener(_onHighQualityClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(_onCloseClicked);
        }

        private void OnVolumeChanged(float value) {
            // AudioListener.volume is static and expects 0..1
            AudioListener.volume = Mathf.Clamp01(value / 100f);
            UpdateVolumeLabel(value);
        }

        private void UpdateVolumeLabel(float value) {
            if (volumeLabel != null) {
                volumeLabel.text = Mathf.RoundToInt(value).ToString();
            }
        }

        private void OnBrightnessChanged(float value) {
            UpdateBrightnessLabel(value);
            ApplyBrightness(value);
        }

        private void UpdateBrightnessLabel(float value) {
            if (brightnessLabel != null) {
                brightnessLabel.text = Mathf.RoundToInt(value).ToString();
            }
        }

        private void ApplyBrightness(float brightness) {
            // Store value so the field is used and follows naming convention
            _currentBrightness = Mathf.Clamp(brightness, 0f, 100f);

            // Example application options (choose one depending on your project):
            // 1) If you have a global shader property for brightness:
            // Shader.SetGlobalFloat("_GlobalBrightness", _currentBrightness / 100f);
            //
            // 2) If you use PostProcessing v2 and ColorGrading, set postExposure:
            // var volume = FindObjectOfType<UnityEngine.Rendering.PostProcessing.PostProcessVolume>();
            // if (volume != null && volume.profile.TryGetSettings(out UnityEngine.Rendering.PostProcessing.ColorGrading cg)) {
            //     cg.postExposure.value = Mathf.Lerp(-1f, 1f, _currentBrightness / 100f);
            // }
            //
            // 3) Or adjust a UI overlay CanvasGroup alpha, camera exposure, or ambient light as appropriate.
        }

        private void SetGraphicsQuality(int qualityLevel) {
            QualitySettings.SetQualityLevel(qualityLevel, true);
            UpdateQualityButtonStates(qualityLevel);
        }

        private void OnCloseClicked() {
            gameObject.SetActive(false);
        }

        private void UpdateQualityButtonStates(int activeQuality) {
            ColorBlock normalColors = new ColorBlock {
                normalColor = new Color(0.2f, 0.2f, 0.26f, 1f),
                highlightedColor = new Color(0.25f, 0.25f, 0.3f, 1f),
                pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f),
                selectedColor = new Color(0.25f, 0.25f, 0.3f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            ColorBlock activeColors = new ColorBlock {
                normalColor = new Color(0.4f, 0.6f, 0.8f, 1f),
                highlightedColor = new Color(0.5f, 0.7f, 0.9f, 1f),
                pressedColor = new Color(0.3f, 0.5f, 0.7f, 1f),
                selectedColor = new Color(0.5f, 0.7f, 0.9f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            if (lowQualityButton != null) lowQualityButton.colors = (activeQuality == 0) ? activeColors : normalColors;
            if (mediumQualityButton != null) mediumQualityButton.colors = (activeQuality == 1) ? activeColors : normalColors;
            if (highQualityButton != null) highQualityButton.colors = (activeQuality == 2) ? activeColors : normalColors;
        }
    }
}
