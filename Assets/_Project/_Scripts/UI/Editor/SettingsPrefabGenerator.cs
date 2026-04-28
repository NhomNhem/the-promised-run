#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Project._Scripts.UI.Editor {
    public static class SettingsPrefabGenerator {
        private const string PrefabFolder = "Assets/_Project/_Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/SettingsPanel.prefab";

        [MenuItem("Tools/UI/Create Settings Panel Prefab")]
        public static void CreateSettingsPrefab() {
            EnsureFolderExists(PrefabFolder);

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject root = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image), typeof(SettingsPanel));
            Image panelImage = root.GetComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.15f, 0.96f);

            RectTransform rootRectTransform = root.GetComponent<RectTransform>();
            rootRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rootRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rootRectTransform.pivot = new Vector2(0.5f, 0.5f);
            rootRectTransform.sizeDelta = new Vector2(680f, 520f);
            rootRectTransform.anchoredPosition = Vector2.zero;

            SettingsPanel settingsPanel = root.GetComponent<SettingsPanel>();

            // Title
            GameObject titleObject = CreateUiObject("Title", root.transform);
            Text titleText = titleObject.AddComponent<Text>();
            titleText.font = defaultFont;
            titleText.text = "SETTINGS";
            titleText.fontSize = 32;
            titleText.alignment = TextAnchor.UpperCenter;
            titleText.color = Color.white;
            RectTransform titleRectTransform = titleObject.GetComponent<RectTransform>();
            titleRectTransform.anchorMin = new Vector2(0.5f, 1f);
            titleRectTransform.anchorMax = new Vector2(0.5f, 1f);
            titleRectTransform.pivot = new Vector2(0.5f, 1f);
            titleRectTransform.anchoredPosition = new Vector2(0f, -30f);
            titleRectTransform.sizeDelta = new Vector2(600f, 50f);

            // Volume Section
            CreateSliderSection(root.transform, "VolumeSection", "Volume", 50f, 0f, 100f, defaultFont, out Slider volumeSlider, out Text volumeLabel);
            SerializedObject serializedSettings = new SerializedObject(settingsPanel);
            serializedSettings.FindProperty("volumeSlider").objectReferenceValue = volumeSlider;
            serializedSettings.FindProperty("volumeLabel").objectReferenceValue = volumeLabel;

            // Brightness Section
            CreateSliderSection(root.transform, "BrightnessSection", "Brightness", 50f, 0f, 100f, defaultFont, out Slider brightnessSlider, out Text brightnessLabel);
            serializedSettings.FindProperty("brightnessSlider").objectReferenceValue = brightnessSlider;
            serializedSettings.FindProperty("brightnessLabel").objectReferenceValue = brightnessLabel;

            // Graphics Quality Buttons
            GameObject graphicsPanel = CreateUiObject("GraphicsPanel", root.transform);
            VerticalLayoutGroup graphicsLayoutGroup = graphicsPanel.AddComponent<VerticalLayoutGroup>();
            graphicsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            graphicsLayoutGroup.childForceExpandHeight = false;
            graphicsLayoutGroup.childForceExpandWidth = false;
            graphicsLayoutGroup.spacing = 10f;

            RectTransform graphicsPanelRectTransform = graphicsPanel.GetComponent<RectTransform>();
            graphicsPanelRectTransform.anchorMin = new Vector2(0.5f, 0f);
            graphicsPanelRectTransform.anchorMax = new Vector2(0.5f, 0f);
            graphicsPanelRectTransform.pivot = new Vector2(0.5f, 0f);
            graphicsPanelRectTransform.anchoredPosition = new Vector2(0f, 40f);
            graphicsPanelRectTransform.sizeDelta = new Vector2(600f, 100f);

            GameObject graphicsLabelObject = CreateUiObject("Label", graphicsPanel.transform);
            Text graphicsLabelText = graphicsLabelObject.AddComponent<Text>();
            graphicsLabelText.font = defaultFont;
            graphicsLabelText.text = "Graphics Quality";
            graphicsLabelText.fontSize = 20;
            graphicsLabelText.alignment = TextAnchor.MiddleCenter;
            graphicsLabelText.color = Color.white;
            graphicsLabelText.raycastTarget = false;

            // Buttons container - horizontal layout
            GameObject buttonsContainer = CreateUiObject("ButtonsContainer", graphicsPanel.transform);
            HorizontalLayoutGroup buttonsLayoutGroup = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            buttonsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayoutGroup.childForceExpandHeight = false;
            buttonsLayoutGroup.childForceExpandWidth = false;
            buttonsLayoutGroup.spacing = 15f;

            RectTransform buttonsContainerRectTransform = buttonsContainer.GetComponent<RectTransform>();
            buttonsContainerRectTransform.sizeDelta = new Vector2(500f, 50f);

            Button lowButton = CreateQualityButton(buttonsContainer.transform, "LowButton", "Low", defaultFont);
            Button mediumButton = CreateQualityButton(buttonsContainer.transform, "MediumButton", "Medium", defaultFont);
            Button highButton = CreateQualityButton(buttonsContainer.transform, "HighButton", "High", defaultFont);

            serializedSettings.FindProperty("lowQualityButton").objectReferenceValue = lowButton;
            serializedSettings.FindProperty("mediumQualityButton").objectReferenceValue = mediumButton;
            serializedSettings.FindProperty("highQualityButton").objectReferenceValue = highButton;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();

            // Close Button
            Button closeButtonComponent = CreateButton(root.transform, "CloseButton", "Close", defaultFont);
            RectTransform closeButtonRectTransform = closeButtonComponent.GetComponent<RectTransform>();
            closeButtonRectTransform.anchorMin = new Vector2(1f, 1f);
            closeButtonRectTransform.anchorMax = new Vector2(1f, 1f);
            closeButtonRectTransform.pivot = new Vector2(1f, 1f);
            closeButtonRectTransform.anchoredPosition = new Vector2(-20f, -20f);
            closeButtonRectTransform.sizeDelta = new Vector2(100f, 40f);
            serializedSettings.FindProperty("closeButton").objectReferenceValue = closeButtonComponent;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null) {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }

            Debug.Log($"Settings prefab created at {PrefabPath}");
        }

        private static void CreateSliderSection(Transform parent, string sectionName, string label, float defaultValue, float minValue, float maxValue, Font defaultFont, out Slider slider, out Text sliderLabel) {
            GameObject section = CreateUiObject(sectionName, parent);
            HorizontalLayoutGroup layoutGroup = section.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 10f;

            RectTransform sectionRectTransform = section.GetComponent<RectTransform>();
            sectionRectTransform.anchorMin = new Vector2(0.5f, 1f);
            sectionRectTransform.anchorMax = new Vector2(0.5f, 1f);
            sectionRectTransform.pivot = new Vector2(0.5f, 1f);
            sectionRectTransform.sizeDelta = new Vector2(600f, 60f);
            if (sectionName == "VolumeSection") {
                sectionRectTransform.anchoredPosition = new Vector2(0f, -100f);
            } else {
                sectionRectTransform.anchoredPosition = new Vector2(0f, -180f);
            }

            // Label
            GameObject labelObject = CreateUiObject("Label", section.transform);
            Text labelText = labelObject.AddComponent<Text>();
            labelText.font = defaultFont;
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = Color.white;
            labelText.raycastTarget = false;
            RectTransform labelRectTransform = labelObject.GetComponent<RectTransform>();
            labelRectTransform.sizeDelta = new Vector2(150f, 40f);

            // Slider
            GameObject sliderObject = CreateUiObject("Slider", section.transform);
            Image sliderBg = sliderObject.AddComponent<Image>();
            sliderBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            slider = sliderObject.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            slider.direction = Slider.Direction.LeftToRight;

            // Fill
            GameObject fillObject = CreateUiObject("Fill", sliderObject.transform);
            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            RectTransform fillRectTransform = fillObject.GetComponent<RectTransform>();
            fillRectTransform.anchorMin = new Vector2(0f, 0f);
            fillRectTransform.anchorMax = new Vector2(0f, 1f);
            fillRectTransform.pivot = new Vector2(0f, 0.5f);
            fillRectTransform.offsetMin = Vector2.zero;
            fillRectTransform.offsetMax = Vector2.zero;
            slider.fillRect = fillRectTransform;

            // Handle
            GameObject handleObject = CreateUiObject("Handle", sliderObject.transform);
            Image handleImage = handleObject.AddComponent<Image>();
            handleImage.color = new Color(0.7f, 0.8f, 0.9f, 1f);
            RectTransform handleRectTransform = handleObject.GetComponent<RectTransform>();
            handleRectTransform.sizeDelta = new Vector2(20f, 30f);
            slider.handleRect = handleRectTransform;

            RectTransform sliderRectTransform = sliderObject.GetComponent<RectTransform>();
            sliderRectTransform.sizeDelta = new Vector2(300f, 40f);

            // Value Label
            GameObject valueLabelObject = CreateUiObject("ValueLabel", section.transform);
            sliderLabel = valueLabelObject.AddComponent<Text>();
            sliderLabel.font = defaultFont;
            sliderLabel.text = defaultValue.ToString();
            sliderLabel.fontSize = 18;
            sliderLabel.alignment = TextAnchor.MiddleCenter;
            sliderLabel.color = Color.white;
            sliderLabel.raycastTarget = false;
            RectTransform valueLabelRectTransform = valueLabelObject.GetComponent<RectTransform>();
            valueLabelRectTransform.sizeDelta = new Vector2(60f, 40f);
        }

        private static Button CreateQualityButton(Transform parent, string name, string label, Font defaultFont) {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.26f, 1f);
            Button button = buttonObject.AddComponent<Button>();

            RectTransform buttonRectTransform = buttonObject.GetComponent<RectTransform>();
            buttonRectTransform.sizeDelta = new Vector2(150f, 40f);

            GameObject labelObject = CreateUiObject("Label", buttonObject.transform);
            Text labelText = labelObject.AddComponent<Text>();
            labelText.font = defaultFont;
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            RectTransform labelRectTransform = labelObject.GetComponent<RectTransform>();
            StretchFullScreen(labelRectTransform);

            return button;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font defaultFont) {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.26f, 1f);
            Button button = buttonObject.AddComponent<Button>();

            RectTransform buttonRectTransform = buttonObject.GetComponent<RectTransform>();
            buttonRectTransform.sizeDelta = new Vector2(100f, 40f);

            GameObject labelObject = CreateUiObject("Label", buttonObject.transform);
            Text labelText = labelObject.AddComponent<Text>();
            labelText.font = defaultFont;
            labelText.text = label;
            labelText.fontSize = 18;
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

