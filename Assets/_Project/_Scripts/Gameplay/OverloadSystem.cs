using UnityEngine;
using System;
using OpenUtility.Data;

namespace ThePromisedRun.Gameplay {
    /// <summary>
    /// OverloadSystem — standalone chaos/overload state machine.
    /// Bridges with PlayerController.ChaosMeter via ScriptableFloat.
    ///
    /// Can be used standalone OR alongside PlayerController:
    ///   - Standalone: call AddGauge() directly
    ///   - Bridge mode: subscribe to _chaosMeterVar.ValueChanged → sync gauge
    /// </summary>
    public class OverloadSystem : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private float _gaugeDuration    = 4f;  // active overload window
        [SerializeField] private float _cooldownDuration = 8f;  // lockout after overload

        [Header("ScriptableVariables (optional bridge)")]
        [SerializeField] private ScriptableFloat _chaosMeterVar; // sync from PlayerController

        // Gauge state
        private float _gauge        = 0f;
        private bool  _isActive     = false;
        private float _activeTimer  = 0f;
        private float _cooldownTimer = 0f;

        // Events
        public event Action<float> OnGaugeChanged;    // gauge value 0–100
        public event Action        OnOverloadTriggered;
        public event Action        OnOverloadEnded;

        public float Gauge            => _gauge;
        public bool  IsActive         => _isActive;
        public float CooldownRemaining => _cooldownTimer;

        private void OnEnable() {
            if (_chaosMeterVar != null)
                _chaosMeterVar.ValueChanged.AddListener(OnChaosMeterChanged);
            // Also sync overload state from PlayerController via ScriptableBool
            // (PlayerController.InitiateOverload sets _overloadStateVar = true)
        }

        private void OnDisable() {
            if (_chaosMeterVar != null)
                _chaosMeterVar.ValueChanged.RemoveListener(OnChaosMeterChanged);
        }

        private void OnChaosMeterChanged(float chaosValue) {
            // Sync gauge from PlayerController's chaos meter (0–100)
            // Don't override if already active or in cooldown
            if (_isActive || _cooldownTimer > 0f) return;
            _gauge = Mathf.Clamp(chaosValue, 0f, 100f);
            OnGaugeChanged?.Invoke(_gauge);
            if (_gauge >= 100f) TriggerOverload();
        }

        private void Update() {
            if (_isActive) {
                _activeTimer -= Time.deltaTime;
                if (_activeTimer <= 0f) EndOverload();
            }

            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        /// <summary>Add gauge amount. Ignored during cooldown.</summary>
        public void AddGauge(float amount) {
            if (_cooldownTimer > 0f) return;
            _gauge = Mathf.Min(_gauge + amount, 100f);
            OnGaugeChanged?.Invoke(_gauge);
            if (_gauge >= 100f) TriggerOverload();
        }

        /// <summary>Passive accumulation per second (call while popup visible).</summary>
        public void AddGaugePerSecond(float rate) {
            if (_cooldownTimer > 0f) return;
            AddGauge(rate * Time.deltaTime);
        }

        private void TriggerOverload() {
            if (_isActive) return;
            _isActive    = true;
            _activeTimer = _gaugeDuration;
            _gauge       = 100f;
            OnOverloadTriggered?.Invoke();
        }

        private void EndOverload() {
            _isActive      = false;
            _gauge         = 0f;
            _cooldownTimer = _cooldownDuration;
            OnOverloadEnded?.Invoke();
        }

        public void SetGaugeForTesting(float value) {
            _gauge = Mathf.Clamp(value, 0f, 100f);
            OnGaugeChanged?.Invoke(_gauge);
        }
    }
}
