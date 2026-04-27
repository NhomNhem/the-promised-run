using UnityEngine;
using UnityEngine.Events;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy {
    public class EnemyHealth : MonoBehaviour, IDamageable {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 30f;

        [Header("Events")]
        public UnityEvent<float> OnDamaged = new UnityEvent<float>(); // normalized 0-1
        public UnityEvent        OnDied    = new UnityEvent();

        private float _currentHealth;
        private bool  _isDead;

        public bool IsAlive => !_isDead;

        private void Awake() {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float amount, DamageInfo info) {
            if (_isDead) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnDamaged.Invoke(_currentHealth / _maxHealth);

            if (_currentHealth <= 0f) {
                _isDead = true;
                OnDied.Invoke();
            }
        }
    }
}
