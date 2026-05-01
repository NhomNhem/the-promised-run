using UnityEngine;
using UnityEngine.UIElements;

namespace ThePromisedRun.UI {
    /// <summary>
    /// VisualElement representing a single floating damage number.
    /// Animates upward and fades out over its lifetime.
    /// </summary>
    public class DamagePopup : VisualElement {
        private Label _label;
        private float _lifetime;
        private float _elapsed;
        private float _floatSpeed;
        private float _startY;

        public DamagePopup() {
            _label = new Label();
            _label.style.color = new StyleColor(Color.white);
            _label.style.fontSize = 24;
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(_label);

            style.position = Position.Absolute;
            style.opacity = 1f;
        }

        public void Initialize(float damage, Vector2 screenPos, float lifetime = 1f, float floatSpeed = 50f) {
            _label.text = Mathf.RoundToInt(damage).ToString();
            _lifetime = lifetime;
            _elapsed = 0f;
            _floatSpeed = floatSpeed;
            _startY = screenPos.y;

            style.left = new StyleLength(new Length(screenPos.x - 20f, LengthUnit.Pixel));
            style.top = new StyleLength(new Length(screenPos.y, LengthUnit.Pixel));
            style.opacity = 1f;
        }

        public bool Update(float deltaTime) {
            _elapsed += deltaTime;
            if (_elapsed >= _lifetime)
                return false;

            float t = _elapsed / _lifetime;
            float newY = _startY - _floatSpeed * _elapsed;
            style.top = new StyleLength(new Length(newY, LengthUnit.Pixel));
            style.opacity = 1f - t;
            return true;
        }
    }
}
