using UnityEngine;
using UnityEngine.Events;
using OpenUtility.Data;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Juice;

namespace ThePromisedRun.Gameplay.Combat {
    /// <summary>
    /// Player health + IDamageable implementation.
    /// Handles hit reaction, invincibility frames, and death.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, IDamageable {
        [Header("Stats")]
        [SerializeField] private float maxHealth   = 100f;
        [SerializeField] private float iFrames     = 0.6f;  // invincibility after hit

        [Header("Hit Reaction")]
        [SerializeField] private float hitKnockback = 4f;
        [SerializeField] private float cameraShakeStrength = 0.15f;
        [SerializeField] private float cameraShakeDuration = 0.2f;

        [Header("ScriptableVariables")]
        [SerializeField] private ScriptableFloat _healthVar;

        [Header("Death")]
        [SerializeField] private ThePromisedRun.UI.DeathScreen _deathScreen;

        [Header("Events")]
        public UnityEvent<float> OnHealthChangedUnity = new UnityEvent<float>(); // normalized 0-1
        public UnityEvent        OnHit           = new UnityEvent();
        public UnityEvent        OnDeathUnity    = new UnityEvent();

        private PlayerController _player;
        private float            _health;
        private float            _iFrameTimer;

        public bool  IsAlive      => _health > 0f;
        public float HealthNorm   => _health / maxHealth;
        public bool  IsInvincible => _iFrameTimer > 0f;
        public float Health        => _health;
        public float MaxHealth     => maxHealth;
        public System.Action<float> OnHealthChanged { get; set; }
        public System.Action OnDeath { get; set; }

        private void Awake() {
            _player = GetComponent<PlayerController>();
            _health = maxHealth;
            
            // Initialize IDamageable events to wrap UnityEvents
            OnHealthChanged = (health) => OnHealthChangedUnity.Invoke(health);
            OnDeath = () => OnDeathUnity.Invoke();
        }

        private void Update() {
            if (_iFrameTimer > 0f) _iFrameTimer -= Time.deltaTime;
        }

        public void TakeDamage(float amount, DamageInfo info) {
            if (!IsAlive || IsInvincible) return;

            // Overload = invincible
            if (_player.IsOverloaded) return;

            _health = Mathf.Max(0f, _health - amount);
            _iFrameTimer = iFrames;

            _healthVar?.SetValue(_health);

            OnHealthChangedUnity.Invoke(HealthNorm);
            OnHit.Invoke();

            // Knockback away from attacker
            if (info.Attacker != null) {
                Vector3 knockDir = (transform.position - info.Attacker.transform.position).normalized;
                knockDir.y = 0f;
                _player.Rb.linearVelocity = knockDir * hitKnockback + Vector3.up * 2f;
            }

            // Camera shake
            ShakeCamera(cameraShakeStrength, cameraShakeDuration);

            // Add chaos — being hit by enemy adds chaos (system is winning = player gets angrier)
            _player.AddChaos(10f, ChaosSource.EnemyHit);

            if (!IsAlive) {
                OnDeathUnity.Invoke();
                // Lazy-find DeathScreen at death time (may be in additively loaded Scene_HUD)
                if (_deathScreen == null)
                    _deathScreen = FindFirstObjectByType<ThePromisedRun.UI.DeathScreen>();
                _deathScreen?.Show();
                Debug.Log("[PlayerHealth] Player died.");
            }
        }

        private void ShakeCamera(float strength, float duration) {
            // Camera.main is safe — Camera lives in Scene_GamePlay with Player.
            // Re-cache if null (e.g. after scene reload).
            var cam = Camera.main?.transform;
            if (cam == null) return;
            StartCoroutine(CameraShakeRoutine(cam, strength, duration));
        }

        private System.Collections.IEnumerator CameraShakeRoutine(
            Transform cam, float strength, float duration) {
            Vector3 origin = cam.localPosition;
            float elapsed = 0f;
            while (elapsed < duration) {
                float t = 1f - (elapsed / duration);
                cam.localPosition = origin + (Vector3)UnityEngine.Random.insideUnitCircle * strength * t;
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.localPosition = origin;
        }

        public void Heal(float amount) {
            _health = Mathf.Min(_health + amount, maxHealth);
            _healthVar?.SetValue(_health);
            OnHealthChangedUnity.Invoke(HealthNorm);
        }

        /// <summary>Restore to a specific HP value (used by checkpoint respawn).</summary>
        public void RestoreHealth(float hp) {
            _health      = Mathf.Clamp(hp, 0f, maxHealth);
            _iFrameTimer = 0f;
            _healthVar?.SetValue(_health);
            OnHealthChangedUnity.Invoke(HealthNorm);
        }
    }
}
