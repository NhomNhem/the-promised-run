using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using ThePromisedRun.Core.Interfaces;
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

        private Animator _animator;
        private Renderer _renderer;

        private static readonly int HitTrigger = Animator.StringToHash("Hit");

        public bool IsAlive => !_isDead;
        public float Health => _currentHealth;
        public float MaxHealth => _maxHealth;
        public System.Action<float> OnHealthChanged { get; set; } = (health) => { };
        public System.Action OnDeath { get; set; } = () => { };

        private void Awake() {
            _currentHealth = _maxHealth;
            _animator      = GetComponentInChildren<Animator>();
            _renderer      = GetComponentInChildren<Renderer>();
        }

        public void TakeDamage(float amount, DamageInfo info) {
            if (_isDead) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnDamaged.Invoke(_currentHealth / _maxHealth);

            if (!_isDead) {
                _animator?.SetTrigger(HitTrigger);
                if (_renderer != null) StartCoroutine(MaterialFlash());
            }

            if (_currentHealth <= 0f) {
                _isDead = true;
                OnDied.Invoke();
            }
        }

        private IEnumerator MaterialFlash() {
            Color original = _renderer.material.color;
            _renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            _renderer.material.color = original;
        }
    }
}
