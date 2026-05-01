using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy {
    public class EnemyHealth : MonoBehaviour, IDamageable {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 30f;
        [SerializeField] private bool _useFixedHitsToKillForTesting = true;
        [SerializeField] private int _hitsToKill = 3;

        [Header("Events")]
        public UnityEvent<float> OnDamaged = new UnityEvent<float>(); // normalized 0-1
        public UnityEvent        OnDied    = new UnityEvent();

        private float _currentHealth;
        private int   _remainingHits;
        private bool  _isDead;

        private Animator             _animator;
        private Renderer             _renderer;
        private MaterialPropertyBlock _propBlock;

        private static readonly int HitTrigger  = Animator.StringToHash("Hit");
        // URP uses _BaseColor; Standard shader uses _Color — _BaseColor works for both URP Lit and Simple Lit
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public bool IsAlive => !_isDead;
        public float Health => _currentHealth;
        public float MaxHealth => _maxHealth;
        public System.Action<float> OnHealthChanged { get; set; } = (health) => { };
        public System.Action OnDeath { get; set; } = () => { };

        private void Awake() {
            _currentHealth = _maxHealth;
            _remainingHits = Mathf.Max(1, _hitsToKill);
            _animator      = GetComponentInChildren<Animator>();
            _renderer      = GetComponentInChildren<Renderer>();
            _propBlock     = new MaterialPropertyBlock();
        }

        public void TakeDamage(float amount, DamageInfo info) {
            if (_isDead) return;

            if (_useFixedHitsToKillForTesting) {
                _remainingHits = Mathf.Max(0, _remainingHits - 1);
                _currentHealth = _maxHealth * ((float)_remainingHits / Mathf.Max(1, _hitsToKill));
            } else {
                _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            }
            UI.DamagePopupSpawner.Spawn(transform.position, amount, UI.DamagePopupType.Enemy);
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
            // Read current color from property block (or fall back to sharedMaterial)
            // Using MaterialPropertyBlock avoids creating a new material instance each call
            _renderer.GetPropertyBlock(_propBlock);
            Color original = _propBlock.GetColor(BaseColorId);
            if (original == Color.clear) {
                // Property block has no override yet — read from shared material
                original = _renderer.sharedMaterial != null
                    ? _renderer.sharedMaterial.GetColor(BaseColorId)
                    : Color.white;
            }

            // Flash red to clearly communicate damage received
            _propBlock.SetColor(BaseColorId, new Color(1f, 0.1f, 0.1f));
            _renderer.SetPropertyBlock(_propBlock);

            yield return new WaitForSeconds(0.1f);

            // Restore original color
            _propBlock.SetColor(BaseColorId, original);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
