using UnityEngine;
using UnityEngine.UI;
using OpenUtility.Data;

namespace ThePromisedRun.UI {
    /// <summary>
    /// OL Gauge HUD — 4 visual states based on GDD §6.2:
    ///   0–30%  : Gray (safe)
    ///   30–70% : Amber flicker (building)
    ///   70–99% : Red + vignette (critical)
    ///   100%   : White flash (overload)
    /// Binds to ScriptableFloat _chaosMeterVar via ValueChanged event.
    /// </summary>
    public class OLGaugeUI : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _trackImage;
        [SerializeField] private Image _vignetteImage;   // full-screen red vignette
        [SerializeField] private Image _flashImage;      // full-screen white flash

        [Header("ScriptableVariable")]
        [SerializeField] private ScriptableFloat _chaosMeterVar;

        [Header("Colors")]
        [SerializeField] private Color _colorSafe     = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _colorBuilding = new Color(1f, 0.7f, 0f);
        [SerializeField] private Color _colorCritical = new Color(1f, 0.15f, 0.1f);
        [SerializeField] private Color _colorOverload = Color.white;

        [Header("Flicker")]
        [SerializeField] private float _flickerSpeed = 8f;

        private float _currentNorm;   // 0–1
        private float _flashTimer;
        private const float FlashDuration = 0.12f;

        private void OnEnable() {
            if (_chaosMeterVar != null)
                _chaosMeterVar.ValueChanged.AddListener(OnChaosChanged);
        }

        private void OnDisable() {
            if (_chaosMeterVar != null)
                _chaosMeterVar.ValueChanged.RemoveListener(OnChaosChanged);
        }

        private void OnChaosChanged(float value) {
            // Assume max = 100
            _currentNorm = Mathf.Clamp01(value / 100f);

            if (_currentNorm >= 1f) TriggerOverloadFlash();
        }

        private void Update() {
            if (_fillImage == null) return;

            _fillImage.fillAmount = _currentNorm;

            // State-based color
            Color targetColor;
            if (_currentNorm >= 1f)       targetColor = _colorOverload;
            else if (_currentNorm >= 0.7f) targetColor = _colorCritical;
            else if (_currentNorm >= 0.3f) targetColor = _colorBuilding;
            else                           targetColor = _colorSafe;

            // Flicker in building/critical states
            if (_currentNorm >= 0.3f && _currentNorm < 1f) {
                float flicker = 0.85f + 0.15f * Mathf.Sin(Time.time * _flickerSpeed);
                targetColor.a = flicker;
            }

            _fillImage.color = targetColor;

            // Vignette
            if (_vignetteImage != null) {
                float vigAlpha = _currentNorm >= 0.7f
                    ? Mathf.Lerp(0f, 0.35f, (_currentNorm - 0.7f) / 0.3f)
                    : 0f;
                Color vc = _vignetteImage.color;
                vc.a = vigAlpha;
                _vignetteImage.color = vc;
            }

            // Flash countdown
            if (_flashTimer > 0f) {
                _flashTimer -= Time.deltaTime;
                if (_flashImage != null) {
                    Color fc = _flashImage.color;
                    fc.a = _flashTimer / FlashDuration;
                    _flashImage.color = fc;
                }
            } else if (_flashImage != null && _flashImage.color.a > 0f) {
                Color fc = _flashImage.color;
                fc.a = 0f;
                _flashImage.color = fc;
            }
        }

        private void TriggerOverloadFlash() {
            _flashTimer = FlashDuration;
            if (_flashImage != null) {
                Color fc = _flashImage.color;
                fc.a = 1f;
                _flashImage.color = fc;
            }
        }
    }
}
