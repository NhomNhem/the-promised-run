using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace _Project._Scripts.UI {
    public class MainMenu : MonoBehaviour {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Scene")]
        [SerializeField] private string gameplaySceneName = "MoveScene";

        private void Awake() {
            if (!HasWiredUi()) {
                BuildRuntimeUi();
            } else {
                WireupCloseButtons();
            }

            HidePanels();
        }

        private void OnEnable() {
            if (playButton != null) playButton.onClick.AddListener(PlayGame);
            if (creditsButton != null) creditsButton.onClick.AddListener(ShowCredits);
            if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDisable() {
            if (playButton != null) playButton.onClick.RemoveListener(PlayGame);
            if (creditsButton != null) creditsButton.onClick.RemoveListener(ShowCredits);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(ShowSettings);
            if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
        }

        private bool HasWiredUi() {
            return playButton != null && creditsButton != null && settingsButton != null && quitButton != null && creditsPanel != null && settingsPanel != null;
        }

        private void BuildRuntimeUi() {
            EnsureEventSystemExists();

            GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRectTransform = canvasObject.GetComponent<RectTransform>();
            StretchFullScreen(canvasRectTransform);

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject backgroundObject = CreateUiObject("Background", canvasObject.transform);
            Image backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            StretchFullScreen(backgroundObject.GetComponent<RectTransform>());

            GameObject titleObject = CreateUiObject("Title", canvasObject.transform);
            Text titleText = titleObject.AddComponent<Text>();
            titleText.font = defaultFont;
            titleText.text = "THE PROMISED RUN";
            titleText.fontSize = 48;
            titleText.alignment = TextAnchor.UpperCenter;
            titleText.color = Color.white;
            RectTransform titleRectTransform = titleObject.GetComponent<RectTransform>();
            titleRectTransform.anchorMin = new Vector2(0.5f, 1f);
            titleRectTransform.anchorMax = new Vector2(0.5f, 1f);
            titleRectTransform.pivot = new Vector2(0.5f, 1f);
            titleRectTransform.anchoredPosition = new Vector2(0f, -80f);
            titleRectTransform.sizeDelta = new Vector2(900f, 80f);

            GameObject buttonsPanelObject = CreateUiObject("ButtonsPanel", canvasObject.transform);
            Image buttonsPanelImage = buttonsPanelObject.AddComponent<Image>();
            buttonsPanelImage.color = new Color(0f, 0f, 0f, 0.25f);
            VerticalLayoutGroup buttonsLayoutGroup = buttonsPanelObject.AddComponent<VerticalLayoutGroup>();
            buttonsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayoutGroup.childForceExpandHeight = false;
            buttonsLayoutGroup.childForceExpandWidth = false;
            buttonsLayoutGroup.spacing = 14f;
            ContentSizeFitter buttonsContentSizeFitter = buttonsPanelObject.AddComponent<ContentSizeFitter>();
            buttonsContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonsContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform buttonsPanelRectTransform = buttonsPanelObject.GetComponent<RectTransform>();
            buttonsPanelRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            buttonsPanelRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            buttonsPanelRectTransform.pivot = new Vector2(0.5f, 0.5f);
            buttonsPanelRectTransform.anchoredPosition = new Vector2(0f, -20f);
            buttonsPanelRectTransform.sizeDelta = new Vector2(360f, 340f);

            playButton = CreateButton(buttonsPanelObject.transform, "PlayButton", "Play", defaultFont);
            creditsButton = CreateButton(buttonsPanelObject.transform, "CreditsButton", "Credits", defaultFont);
            settingsButton = CreateButton(buttonsPanelObject.transform, "SettingsButton", "Settings", defaultFont);
            quitButton = CreateButton(buttonsPanelObject.transform, "QuitButton", "Quit", defaultFont);

            creditsPanel = CreatePanel(canvasObject.transform, "CreditsPanel", defaultFont, "CREDITS\n\nDesign, code, art, and audio placeholders can be replaced here.");
            settingsPanel = CreatePanel(canvasObject.transform, "SettingsPanel", defaultFont, "SETTINGS\n\nPut sliders, toggles, and audio options here.");
            creditsPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        #region Button Actions

        public void PlayGame() {
            if (string.IsNullOrWhiteSpace(gameplaySceneName)) {
                Debug.LogError("MainMenu: Gameplay scene name is missing.");
                return;
            }

            SceneManager.LoadScene(gameplaySceneName);
        }

        public void ShowCredits() {
            if (creditsPanel == null) {
                Debug.LogWarning("MainMenu: Credits panel is not assigned.");
                return;
            }

            if (creditsPanel.activeSelf) {
                HidePanels();
                return;
            }

            if (settingsPanel != null) settingsPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }

        public void ShowSettings() {
            if (settingsPanel == null) {
                Debug.LogWarning("MainMenu: Settings panel is not assigned.");
                return;
            }

            if (settingsPanel.activeSelf) {
                HidePanels();
                return;
            }

            if (creditsPanel != null) creditsPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        public void HidePanels() {
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void QuitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region UI Builder

        private void WireupCloseButtons() {
            Button[] allButtons = GetComponentsInChildren<Button>();
            foreach (Button btn in allButtons) {
                if (btn.gameObject.name == "CloseButton") {
                    btn.onClick.AddListener(HidePanels);
                }
            }

            // Find any buttons in panels that aren't main menu buttons and wire them as close buttons
            if (creditsPanel != null) {
                WireupCloseButtonsInPanel(creditsPanel);
            }
            if (settingsPanel != null) {
                WireupCloseButtonsInPanel(settingsPanel);
            }
        }

        private void WireupCloseButtonsInPanel(GameObject panel) {
            Button[] panelButtons = panel.GetComponentsInChildren<Button>();
            foreach (Button btn in panelButtons) {
                // Skip the main menu buttons
                if (btn == playButton || btn == creditsButton || btn == settingsButton || btn == quitButton) {
                    continue;
                }
                // Wire this button as a close button
                btn.onClick.AddListener(HidePanels);
            }
        }


        private GameObject CreatePanel(Transform parent, string panelName, Font defaultFont, string text) {
            GameObject panel = CreateUiObject(panelName, parent);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.15f, 0.96f);

            RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
            panelRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            panelRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panelRectTransform.pivot = new Vector2(0.5f, 0.5f);
            panelRectTransform.sizeDelta = new Vector2(680f, 420f);
            panelRectTransform.anchoredPosition = Vector2.zero;

            GameObject headerObject = CreateUiObject("Header", panel.transform);
            Text headerText = headerObject.AddComponent<Text>();
            headerText.font = defaultFont;
            headerText.text = text;
            headerText.fontSize = 26;
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.color = Color.white;
            headerText.horizontalOverflow = HorizontalWrapMode.Wrap;
            headerText.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform headerRectTransform = headerObject.GetComponent<RectTransform>();
            headerRectTransform.anchorMin = new Vector2(0f, 0f);
            headerRectTransform.anchorMax = new Vector2(1f, 1f);
            headerRectTransform.offsetMin = new Vector2(40f, 40f);
            headerRectTransform.offsetMax = new Vector2(-40f, -40f);

            // Close button in top-right corner of panel
            Button closeButtonComponent = CreateButton(panel.transform, "CloseButton", "Close", defaultFont);
            closeButtonComponent.onClick.AddListener(HidePanels);
            closeButtonComponent.transition = Selectable.Transition.ColorTint;
            ColorBlock closeButtonColors = closeButtonComponent.colors;
            closeButtonColors.normalColor = new Color(0.15f, 0.15f, 0.2f, 1f);
            closeButtonColors.highlightedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            closeButtonColors.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            closeButtonColors.selectedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            closeButtonComponent.colors = closeButtonColors;
            Navigation closeNav = new Navigation { mode = Navigation.Mode.None };
            closeButtonComponent.navigation = closeNav;
            RectTransform closeButtonRectTransform = closeButtonComponent.GetComponent<RectTransform>();
            closeButtonRectTransform.anchorMin = new Vector2(1f, 1f);
            closeButtonRectTransform.anchorMax = new Vector2(1f, 1f);
            closeButtonRectTransform.pivot = new Vector2(1f, 1f);
            closeButtonRectTransform.anchoredPosition = new Vector2(-20f, -20f);
            closeButtonRectTransform.sizeDelta = new Vector2(100f, 40f);

            return panel;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font defaultFont) {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.26f, 1f);
            Button button = buttonObject.AddComponent<Button>();

            RectTransform buttonRectTransform = buttonObject.GetComponent<RectTransform>();
            buttonRectTransform.sizeDelta = new Vector2(320f, 64f);

            GameObject labelObject = CreateUiObject("Label", buttonObject.transform);
            Text labelText = labelObject.AddComponent<Text>();
            labelText.font = defaultFont;
            labelText.text = label;
            labelText.fontSize = 30;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            RectTransform labelRectTransform = labelObject.GetComponent<RectTransform>();
            StretchFullScreen(labelRectTransform);

            return button;
        }

        private static GameObject CreateUiObject(string name, Transform parent) {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void StretchFullScreen(RectTransform rectTransform) {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void EnsureEventSystemExists() {
            if (Object.FindFirstObjectByType<EventSystem>() != null) {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        #endregion
    }
}
