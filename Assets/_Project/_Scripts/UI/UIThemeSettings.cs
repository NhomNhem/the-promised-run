using UnityEngine;
using UnityEngine.UIElements;

namespace ThePromisedRun.UI {
    [CreateAssetMenu(fileName = "UIThemeSettings", menuName = "ThePromisedRun/UI Theme Settings")]
    public class UIThemeSettings : ScriptableObject {
        [Header("Colors - Fantasy Theme")]
        [SerializeField] private Color _primaryColor = new Color(0.45f, 0.25f, 0.65f);
        [SerializeField] private Color _secondaryColor = new Color(0.3f, 0.3f, 0.4f);
        [SerializeField] private Color _accentColor = new Color(0.85f, 0.65f, 0.15f);
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _textShadowColor = new Color(0.1f, 0.1f, 0.15f);

        [Header("Button Colors")]
        [SerializeField] private Color _buttonNormalColor = new Color(0.2f, 0.2f, 0.25f);
        [SerializeField] private Color _buttonHoverColor = new Color(0.3f, 0.3f, 0.4f);
        [SerializeField] private Color _buttonPressedColor = new Color(0.15f, 0.15f, 0.2f);

        [Header("Panel Colors")]
        [SerializeField] private Color _panelBgColor = new Color(0.12f, 0.12f, 0.18f, 0.95f);
        [SerializeField] private Color _panelBorderColor = new Color(0.4f, 0.35f, 0.5f);

        [Header("Animation Settings")]
        [SerializeField] private float _buttonHoverDuration = 0.2f;
        [SerializeField] private float _buttonPressDuration = 0.1f;
        [SerializeField] private float _panelFadeDuration = 0.3f;
        [SerializeField] private float _buttonScaleHover = 1.1f;
        [SerializeField] private float _buttonScalePress = 0.95f;

        [Header("LayerLabs Sprite Paths")]
        [SerializeField] private string _buttonSpritePath = "Assets/_Project/_Art/UI/Layer Lab/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/Button/";
        [SerializeField] private string _frameSpritePath = "Assets/_Project/_Art/UI/Layer Lab/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/Frame/";
        [SerializeField] private string _labelSpritePath = "Assets/_Project/_Art/UI/Layer Lab/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/Label-Title/";
        [SerializeField] private string _sliderSpritePath = "Assets/_Project/_Art/UI/Layer Lab/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/Slider/";

        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;
        public Color AccentColor => _accentColor;
        public Color TextColor => _textColor;
        public Color TextShadowColor => _textShadowColor;
        public Color ButtonNormalColor => _buttonNormalColor;
        public Color ButtonHoverColor => _buttonHoverColor;
        public Color ButtonPressedColor => _buttonPressedColor;
        public Color PanelBgColor => _panelBgColor;
        public Color PanelBorderColor => _panelBorderColor;
        public float ButtonHoverDuration => _buttonHoverDuration;
        public float ButtonPressDuration => _buttonPressDuration;
        public float PanelFadeDuration => _panelFadeDuration;
        public float ButtonScaleHover => _buttonScaleHover;
        public float ButtonScalePress => _buttonScalePress;

        public string ButtonSpritePath => _buttonSpritePath;
        public string FrameSpritePath => _frameSpritePath;
        public string LabelSpritePath => _labelSpritePath;
        public string SliderSpritePath => _sliderSpritePath;

        public static UIThemeSettings GetOrCreate() {
            UIThemeSettings settings = Resources.Load<UIThemeSettings>("UIThemeSettings");
            if (settings == null) {
                Debug.LogWarning("[UIThemeSettings] No settings asset found, using defaults");
            }
            return settings;
        }
    }
}