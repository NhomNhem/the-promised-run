using TMPro;
using UnityEngine;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Runtime world-space floating damage text.
    /// </summary>
    public class DamagePopupText : MonoBehaviour {
        [SerializeField] private float _lifetime = 0.75f;
        [SerializeField] private float _riseSpeed = 1.4f;
        [SerializeField] private float _driftSpeed = 0.6f;

        private TextMeshPro _tmp;
        private Color _baseColor;
        private Vector3 _driftDirection;
        private float _timer;

        public void Initialize(string text, Color color, float scale = 1f) {
            if (_tmp == null) {
                _tmp = gameObject.GetComponent<TextMeshPro>();
                if (_tmp == null) _tmp = gameObject.AddComponent<TextMeshPro>();
            }

            _tmp.text = text;
            _tmp.fontSize = 5f * Mathf.Max(0.6f, scale);
            _tmp.alignment = TextAlignmentOptions.Center;

            _baseColor = color;
            _tmp.color = _baseColor;

            float randomX = Random.Range(-1f, 1f);
            _driftDirection = new Vector3(randomX, 0f, 0f).normalized;
        }

        private void Update() {
            _timer += Time.deltaTime;

            transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);
            transform.position += _driftDirection * (_driftSpeed * Time.deltaTime);

            Camera cam = Camera.main;
            if (cam != null) {
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            }

            float t = Mathf.Clamp01(_timer / _lifetime);
            Color color = _baseColor;
            color.a = 1f - t;
            _tmp.color = color;

            if (_timer >= _lifetime) {
                Destroy(gameObject);
            }
        }
    }
}
