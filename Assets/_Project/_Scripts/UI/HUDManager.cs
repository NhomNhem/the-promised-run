using UnityEngine;
using UnityEngine.UIElements;

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
    ///   └── DeathScreen
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [DefaultExecutionOrder(-100)] // runs before other HUD scripts
    public class HUDManager : MonoBehaviour {

        private UIDocument    _doc;
        private VisualElement _root;

        private void Awake() {
            _doc  = GetComponent<UIDocument>();
            _root = _doc?.rootVisualElement;

            if (_root == null) {
                Debug.LogError("[HUDManager] UIDocument rootVisualElement is null. HUD will not initialize.");
                return;
            }

            // Inject shared root into each HUD controller on this GameObject
            GetComponent<GameHUDController>()?.Initialize(_root);
            GetComponent<ComboCounterUI>()?.Initialize(_root);
            GetComponent<PopupUI>()?.Initialize(_root);
            GetComponent<DeathScreen>()?.Initialize(_root);
        }
    }
}
