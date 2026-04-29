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
            // Lazy cache — Camera.main is safe when Camera is in same scene (Scene_GamePlay)
            _cam = Camera.main?.transform;

            var root = _doc?.rootVisualElement;
            _fill = root?.Q<VisualElement>("hp-bar-fill");

            // Use Initialize(EnemyHealth) if called externally; fallback to GetComponentInParent
            var eh = GetComponentInParent<Gameplay.Enemy.EnemyHealth>();
            if (eh != null) BindToEnemyHealth(eh);

            SetFill(1f);
        }

        private void LateUpdate() {
            // Re-cache camera if lost (e.g. scene reload)
            if (_cam == null) _cam = Camera.main?.transform;

            if (_cam != null)
                transform.rotation = Quaternion.Euler(0f, _cam.eulerAngles.y, 0f);

            transform.localPosition = _offset;
        }

        /// <summary>
        /// Optional explicit initialization — call from enemy setup code
        /// to avoid GetComponentInParent coupling.
        /// </summary>
        public void Initialize(Gameplay.Enemy.EnemyHealth enemyHealth, Transform cameraTransform = null) {
            if (cameraTransform != null) _cam = cameraTransform;
            if (enemyHealth != null) BindToEnemyHealth(enemyHealth);
        }

        private void BindToEnemyHealth(Gameplay.Enemy.EnemyHealth eh) {
            eh.OnDamaged.AddListener(SetFill);
            eh.OnDied.AddListener(HideBar);
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
