using UnityEngine;
using UnityEngine.Events;
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

        [Header("Events")]
        public UnityEvent<float> OnHealthChanged = new UnityEvent<float>(); // normalized 0-1
        public UnityEvent        OnHit           = new UnityEvent();
        public UnityEvent        OnDeath         = new UnityEvent();

        private PlayerController _player;
        private float            _health;
        private float            _iFrameTimer;

        public bool  IsAlive      => _health > 0f;
        public float HealthNorm   => _health / maxHealth;
        public bool  IsInvincible => _iFrameTimer > 0f;

        private void Awake() {
            _player = GetComponent<PlayerController>();
            _health = maxHealth;
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

            OnHealthChanged.Invoke(HealthNorm);
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
                OnDeath.Invoke();
                Debug.Log("[PlayerHealth] Player died.");
            }
        }

        private void ShakeCamera(float strength, float duration) {
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
            OnHealthChanged.Invoke(HealthNorm);
        }
    }
}
