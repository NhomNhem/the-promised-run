using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

namespace ThePromisedRun.UI {
    /// <summary>
    /// HUDManager — single bootstrap for the unified HUD UIDocument.
    ///
    /// Architecture:
    ///   One UIDocument (HUD.uxml) → one PanelSettings → one render pass.
    ///   All HUD MonoBehaviours (GameHUDController, PopupUI, DeathScreen,
    ///   ComboCounterUI) live on the same GameObject and share the same
    ///   VisualElement root. HUDManager queries each layer root and injects
    ///   it via Initialize() so each controller never needs its own UIDocument.
    ///
    /// Scene_HUD hierarchy:
    ///   HUD (GameObject)
    ///   ├── UIDocument          ← source: HUD.uxml
    ///   ├── HUDManager          ← this script (runs first via Script Execution Order)
    ///   ├── GameHUDController
    ///   ├── ComboCounterUI
    ///   ├── PopupUI
    ///   ├── DeathScreen
    ///   ├── PauseMenuController
    ///   └── SettingsPanelController
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [DefaultExecutionOrder(-100)] // runs before other HUD scripts
    public class HUDManager : MonoBehaviour {

        [Header("Intro Fade")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeInDelay    = 0.1f;

        private UIDocument    _doc;
        private VisualElement _root;
        private VisualElement _hudRoot;

        private void Awake() {
            _doc  = GetComponent<UIDocument>();

            if (_doc == null) {
                Debug.LogError("[HUDManager] UIDocument not found.");
                return;
            }

            _root = _doc.rootVisualElement;

            if (_root == null) {
                Debug.LogError("[HUDManager] UIDocument rootVisualElement is null. HUD will not initialize.");
                return;
            }

            // Hide hud-root immediately — FadeIn() will animate it in.
            // HUD scene is loaded AFTER loading screen fades out, so this
            // just ensures opacity starts at 0 for the fade animation.
            _hudRoot = _root.Q<VisualElement>("hud-root");
            if (_hudRoot != null) _hudRoot.style.opacity = 0f;

            // Inject shared root into each HUD controller on this GameObject
            GetComponent<GameHUDController>()?.Initialize(_root);
            GetComponent<ComboCounterUI>()?.Initialize(_root);
            GetComponent<PopupUI>()?.Initialize(_root);
            GetComponent<PopupSpawner>()?.Initialize(_root);
            GetComponent<DeathScreen>()?.Initialize(_root);
            GetComponent<OLGaugeController>()?.Initialize(_root);
            GetComponent<EndingSequenceController>()?.Initialize(_root);
            GetComponent<OverloadFlashEffect>()?.Initialize(_root);
            GetComponent<PauseMenuController>()?.Initialize(_root);
            GetComponent<SettingsPanelController>()?.Initialize(_root);
        }

        /// <summary>
        /// Fade the entire HUD in. Called by MainMenuController after the
        /// loading screen has finished and faded out.
        /// HUD scene is loaded just before this call, so _hudRoot is fresh.
        /// </summary>
        public void FadeIn() {
            if (_hudRoot == null) {
                Debug.LogWarning("[HUDManager] FadeIn(): 'hud-root' not found — HUD appears instantly.");
                return;
            }

            _hudRoot.style.opacity = 0f;
            DOTween.To(
                () => _hudRoot.style.opacity.value,
                x  => _hudRoot.style.opacity = x,
                1f, _fadeInDuration
            ).SetEase(Ease.OutCubic)
             .SetDelay(_fadeInDelay)
             .SetUpdate(true);
        }
    }
}
