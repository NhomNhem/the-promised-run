using UnityEngine;
using UnityEngine.UIElements;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Enemy HP bar — UI Toolkit world-space UIDocument.
    /// Billboard faces camera. Driven by EnemyHealth.OnDamaged.
    /// Attach to a child GameObject of the enemy root.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class EnemyHPBarUI : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 2.4f, 0f);

        private UIDocument    _doc;
        private VisualElement _fill;
        private Transform     _cam;
        private float         _currentHP = 1f;

        private void Awake() {
            _doc = GetComponent<UIDocument>();
        }

        private void Start() {
            _cam = Camera.main?.transform;

            var root = _doc?.rootVisualElement;
            _fill = root?.Q<VisualElement>("hp-bar-fill");

            var eh = GetComponentInParent<Gameplay.Enemy.EnemyHealth>();
            if (eh != null) {
                eh.OnDamaged.AddListener(SetFill);
                eh.OnDied.AddListener(HideBar);
            }

            SetFill(1f); // start full
        }

        private void LateUpdate() {
            // Billboard — match camera Y rotation only
            if (_cam != null)
                transform.rotation = Quaternion.Euler(0f, _cam.eulerAngles.y, 0f);

            // Keep offset above enemy root
            transform.localPosition = _offset;
        }

        public void SetFill(float normalizedHP) {
            _currentHP = Mathf.Clamp01(normalizedHP);
            if (_fill == null) return;

            _fill.style.width = new StyleLength(new Length(_currentHP * 100f, LengthUnit.Percent));

            // Color classes
            _fill.RemoveFromClassList("medium");
            _fill.RemoveFromClassList("low");
            if (_currentHP < 0.3f)      _fill.AddToClassList("low");
            else if (_currentHP < 0.6f) _fill.AddToClassList("medium");
        }

        private void HideBar() => gameObject.SetActive(false);
    }
}
