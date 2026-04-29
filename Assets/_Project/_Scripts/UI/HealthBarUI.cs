using UnityEngine;
using UnityEngine.UI;
using OpenUtility.Data;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Player health bar HUD.
    /// Binds to ScriptableFloat _healthVar (absolute HP value).
    /// Shows HP as fill + numeric text.
    /// Flashes red on damage.
    /// </summary>
    public class HealthBarUI : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Image  _fillImage;
        [SerializeField] private TMPro.TextMeshProUGUI _hpText;

        [Header("ScriptableVariable")]
        [SerializeField] private ScriptableFloat _healthVar;

        [Header("Config")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private Color _colorFull    = new Color(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color _colorLow     = new Color(1f, 0.2f, 0.1f);
        [SerializeField] private float _damageFlashDuration = 0.2f;

        private float _flashTimer;
        private float _currentHP;

        private void OnEnable() {
            if (_healthVar != null) {
                _currentHP = _healthVar.GetValue();
                _healthVar.ValueChanged.AddListener(OnHealthChanged);
            }
        }

        private void OnDisable() {
            if (_healthVar != null)
                _healthVar.ValueChanged.RemoveListener(OnHealthChanged);
        }

        private void OnHealthChanged(float value) {
            if (value < _currentHP) _flashTimer = _damageFlashDuration; // took damage
            _currentHP = value;
        }

        private void Update() {
            if (_fillImage == null) return;

            float norm = Mathf.Clamp01(_currentHP / _maxHealth);
            _fillImage.fillAmount = norm;
            _fillImage.color = Color.Lerp(_colorLow, _colorFull, norm);

            // Damage flash
            if (_flashTimer > 0f) {
                _flashTimer -= Time.deltaTime;
                float t = _flashTimer / _damageFlashDuration;
                _fillImage.color = Color.Lerp(_fillImage.color, Color.white, t * 0.6f);
            }

            if (_hpText != null)
                _hpText.text = $"{Mathf.CeilToInt(_currentHP)}/{Mathf.CeilToInt(_maxHealth)}";
        }
    }
}
