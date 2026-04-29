#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace _Project._Scripts.UI.Editor {
    public static class MainMenuPrefabGenerator {
        private const string PrefabFolder = "Assets/_Project/_Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/MainMenu.prefab";

        [MenuItem("Tools/UI/Create Main Menu Prefab")]
        public static void CreateMainMenuPrefab() {
            EnsureFolderExists(PrefabFolder);

            GameObject root = new GameObject("MainMenu", typeof(RectTransform));
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = root.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            MainMenu mainMenu = root.AddComponent<MainMenu>();

            RectTransform rootRectTransform = root.GetComponent<RectTransform>();
            StretchFullScreen(rootRectTransform);

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject background = CreateUIObject("Background", root.transform);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            StretchFullScreen(background.GetComponent<RectTransform>());

            GameObject title = CreateUIObject("Title", root.transform);
            Text titleText = title.AddComponent<Text>();
            titleText.font = defaultFont;
            titleText.text = "THE PROMISED RUN";
            titleText.fontSize = 48;
            titleText.alignment = TextAnchor.UpperCenter;
            titleText.color = Color.white;
            RectTransform titleRectTransform = title.GetComponent<RectTransform>();
            titleRectTransform.anchorMin = new Vector2(0.5f, 1f);
            titleRectTransform.anchorMax = new Vector2(0.5f, 1f);
            titleRectTransform.pivot = new Vector2(0.5f, 1f);
            titleRectTransform.anchoredPosition = new Vector2(0f, -80f);
            titleRectTransform.sizeDelta = new Vector2(900f, 80f);

            GameObject buttonsPanel = CreateUIObject("ButtonsPanel", root.transform);
            Image buttonsPanelImage = buttonsPanel.AddComponent<Image>();
            buttonsPanelImage.color = new Color(0f, 0f, 0f, 0.25f);
            VerticalLayoutGroup buttonsLayoutGroup = buttonsPanel.AddComponent<VerticalLayoutGroup>();
            buttonsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayoutGroup.childForceExpandHeight = false;
            buttonsLayoutGroup.childForceExpandWidth = false;
            buttonsLayoutGroup.spacing = 14f;
            ContentSizeFitter buttonsContentSizeFitter = buttonsPanel.AddComponent<ContentSizeFitter>();
            buttonsContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonsContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            RectTransform buttonsPanelRectTransform = buttonsPanel.GetComponent<RectTransform>();
            buttonsPanelRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            buttonsPanelRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            buttonsPanelRectTransform.pivot = new Vector2(0.5f, 0.5f);
            buttonsPanelRectTransform.anchoredPosition = new Vector2(0f, -20f);
            buttonsPanelRectTransform.sizeDelta = new Vector2(360f, 340f);

            Button playButton = CreateButton(buttonsPanel.transform, "PlayButton", "Play", defaultFont);
            Button creditsButton = CreateButton(buttonsPanel.transform, "CreditsButton", "Credits", defaultFont);
            Button settingsButton = CreateButton(buttonsPanel.transform, "SettingsButton", "Settings", defaultFont);
            Button quitButton = CreateButton(buttonsPanel.transform, "QuitButton", "Quit", defaultFont);

            GameObject creditsPanel = CreatePanel(root.transform, "CreditsPanel", defaultFont, "CREDITS\n\nDesign, code, art, and audio placeholders can be replaced here.");
            GameObject settingsPanel = CreatePanel(root.transform, "SettingsPanel", defaultFont, "SETTINGS\n\nPut sliders, toggles, and audio options here.");
            creditsPanel.SetActive(false);
            settingsPanel.SetActive(false);

            SerializedObject serializedMainMenu = new SerializedObject(mainMenu);
            serializedMainMenu.FindProperty("playButton").objectReferenceValue = playButton;
            serializedMainMenu.FindProperty("creditsButton").objectReferenceValue = creditsButton;
            serializedMainMenu.FindProperty("settingsButton").objectReferenceValue = settingsButton;
            serializedMainMenu.FindProperty("quitButton").objectReferenceValue = quitButton;
            serializedMainMenu.FindProperty("creditsPanel").objectReferenceValue = creditsPanel;
            serializedMainMenu.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            serializedMainMenu.FindProperty("gameplaySceneName").stringValue = "MoveScene";
            serializedMainMenu.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystemExists();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null) {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }

            Debug.Log($"MainMenu prefab created at {PrefabPath}");
        }

        private static GameObject CreatePanel(Transform parent, string name, Font defaultFont, string text) {
            GameObject panel = CreateUIObject(name, parent);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.15f, 0.96f);

            RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
            panelRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            panelRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panelRectTransform.pivot = new Vector2(0.5f, 0.5f);
            panelRectTransform.sizeDelta = new Vector2(680f, 420f);
            panelRectTransform.anchoredPosition = Vector2.zero;

            GameObject header = CreateUIObject("Header", panel.transform);
            Text headerText = header.AddComponent<Text>();
            headerText.font = defaultFont;
            headerText.text = text;
            headerText.fontSize = 26;
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.color = Color.white;
            headerText.horizontalOverflow = HorizontalWrapMode.Wrap;
            headerText.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform headerRectTransform = header.GetComponent<RectTransform>();
            headerRectTransform.anchorMin = new Vector2(0f, 0f);
            headerRectTransform.anchorMax = new Vector2(1f, 1f);
            headerRectTransform.offsetMin = new Vector2(40f, 40f);
            headerRectTransform.offsetMax = new Vector2(-40f, -40f);

            Button closeButtonComponent = CreateButton(panel.transform, "CloseButton", "Close", defaultFont);
            RectTransform closeButtonRectTransform = closeButtonComponent.GetComponent<RectTransform>();
            closeButtonRectTransform.anchorMin = new Vector2(1f, 1f);
            closeButtonRectTransform.anchorMax = new Vector2(1f, 1f);
            closeButtonRectTransform.pivot = new Vector2(1f, 1f);
            closeButtonRectTransform.anchoredPosition = new Vector2(-20f, -20f);
            closeButtonRectTransform.sizeDelta = new Vector2(100f, 40f);

            return panel;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font defaultFont) {
            GameObject buttonObject = CreateUIObject(name, parent);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.26f, 1f);
            Button button = buttonObject.AddComponent<Button>();

            RectTransform buttonRectTransform = buttonObject.GetComponent<RectTransform>();
            buttonRectTransform.sizeDelta = new Vector2(320f, 64f);

            GameObject labelObject = CreateUIObject("Label", buttonObject.transform);
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

        private static GameObject CreateUIObject(string name, Transform parent) {
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

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static void EnsureFolderExists(string folderPath) {
            if (AssetDatabase.IsValidFolder(folderPath)) {
                return;
            }

            string parentFolder = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string folderName = Path.GetFileName(folderPath);

            if (string.IsNullOrEmpty(parentFolder)) {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parentFolder)) {
                EnsureFolderExists(parentFolder);
            }

            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
}
#endif

