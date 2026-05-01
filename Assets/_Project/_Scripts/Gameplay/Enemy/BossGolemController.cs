using UnityEngine;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// BossGolemController - Skeleton Golem boss cho Level07
    /// PURE C# StateMachine (KHÔNG dùng Animator transitions)
    /// 3-phase boss theo GDD §7
    /// </summary>
    public class BossGolemController : MonoBehaviour {
        [Header("Boss Stats - GDD §7")]
        [SerializeField] private float _maxHealth = 150f;
        [SerializeField] private float _attackCooldown = 2f;

        [Header("Phase 2 - Popup Barrage")]
        [SerializeField] private float _popupBarrageInterval = 1.5f;
        [SerializeField] private int   _popupsPerBurst = 2;

        [Header("Visual States")]
        [SerializeField] private Color _colorInvincible = new Color(0.3f, 0.3f, 0.8f);   // Blue
        [SerializeField] private Color _colorVulnerable = new Color(1f, 0.5f, 0f);    // Orange
        [SerializeField] private Color _colorHit = new Color(1f, 1f, 0.5f);          // Yellow flash
        [SerializeField] private float _hitFlashDuration = 0.15f;

        // State machine - PURE C# (KHÔNG dùng Animator transitions)
        public enum BossState { Idle, Battle, Phase1, Phase2, Phase3, Dead }
        private BossState _currentState;

        // Internal state
        private float _health;
        private float _attackTimer;
        private float _popupTimer;
        private bool _isDefeated;
        private float _hitFlashTimer;
        private Material _material;

        // Phase thresholds (GDD §7)
        private const float Phase2Threshold = 0.5f;    // 50% HP
        private const float Phase3Threshold = 0.25f;   // 25% HP

        private void Awake() {
            _health = _maxHealth;
            _currentState = BossState.Idle;
            _attackTimer = 0f;
            _popupTimer = 0f;
            _isDefeated = false;
        }

        private void Start() {
            // Get components
            Animator animator = GetComponentInChildren<Animator>();
            if (animator == null) {
                animator = GetComponent<Animator>();
            }

            // Get material for color changes
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) {
                _material = renderer.material;
                SetVisualState(false); // Start invincible
            }

            Debug.Log("[BossGolem] Initialized - HP: " + _health + ", State: " + _currentState);
        }

        private void Update() {
            if (_isDefeated) return;

            // Update timers
            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;
            if (_hitFlashTimer > 0f) {
                _hitFlashTimer -= Time.deltaTime;
                if (_hitFlashTimer <= 0f && _material != null)
                    _material.color = _colorInvincible;
            }

            // PURE C# StateMachine
            switch (_currentState) {
                case BossState.Idle:
                    UpdateIdle();
                    break;
                case BossState.Battle:
                    UpdateBattle();
                    break;
                case BossState.Phase1:
                    UpdatePhase1();
                    break;
                case BossState.Phase2:
                    UpdatePhase2();
                    break;
                case BossState.Phase3:
                    UpdatePhase3();
                    break;
            }
        }

        private void UpdateIdle() {
            // Wait for player to enter arena
        }

        private void UpdateBattle() {
            // Start battle immediately (since no OverloadSystem reference)
            _currentState = BossState.Phase1;
            Debug.Log("[BossGolem] Battle started - Phase 1!");
        }

        private void UpdatePhase1() {
            // Phase 1 (100-51% HP): Basic attacks
            if (_attackTimer <= 0f) {
                PerformAttack("Attack01");
                _attackTimer = _attackCooldown;
            }
        }

        private void UpdatePhase2() {
            // Phase 2 (50-26% HP): Popup barrage + faster attacks
            _popupTimer += Time.deltaTime;
            if (_popupTimer >= _popupBarrageInterval) {
                _popupTimer = 0f;
                Debug.Log("[BossGolem] Spawning " + _popupsPerBurst + " popups!");
            }

            if (_attackTimer <= 0f) {
                PerformAttack("Attack02");
                _attackTimer = _attackCooldown * 0.8f; // Faster in phase 2
            }
        }

        private void UpdatePhase3() {
            // Phase 3 (25-0% HP): More aggressive
            if (_attackTimer <= 0f) {
                PerformAttack("Attack01");
                _attackTimer = _attackCooldown * 0.6f; // Even faster
            }
        }

        public void TakeDamage(float damage) {
            if (_isDefeated) return;

            _health -= damage;
            _health = Mathf.Max(0, _health);

            Debug.Log("[BossGolem] Hit! HP: " + _health + "/" + _maxHealth);

            // Visual feedback
            if (_material != null) {
                _material.color = _colorHit;
                _hitFlashTimer = _hitFlashDuration;
            }

            // Trigger hit animation (pure C# - no animator transition)
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) {
                animator.SetTrigger("Hit");
            }

            // Update phase based on HP
            UpdatePhaseBasedOnHP();

            if (_health <= 0) {
                DefeatBoss();
            }
        }

        private void UpdatePhaseBasedOnHP() {
            float hpPercent = _health / _maxHealth;

            if (hpPercent > Phase2Threshold)
                _currentState = BossState.Phase1;
            else if (hpPercent > Phase3Threshold)
                _currentState = BossState.Phase2;
            else
                _currentState = BossState.Phase3;

            Debug.Log("[BossGolem] Phase changed to: " + _currentState);
        }

        private void PerformAttack(string attackTrigger) {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) {
                animator.SetTrigger(attackTrigger);
            }
            Debug.Log("[BossGolem] Performing attack: " + attackTrigger);
        }

        private void SetVisualState(bool vulnerable) {
            if (_material == null) return;

            _material.color = vulnerable ? _colorVulnerable : _colorInvincible;

            // Scale change for visual feedback
            Vector3 baseScale = new Vector3(2f, 2f, 2f);
            transform.localScale = vulnerable
                ? baseScale * 1.1f  // Slightly larger when vulnerable
                : baseScale;
        }

        private void DefeatBoss() {
            if (_isDefeated) return;
            _isDefeated = true;

            Debug.Log("[BossGolem] DEFEATED!");

            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) {
                animator.SetTrigger("Death");
            }

            if (_material != null) {
                _material.color = Color.gray;
            }
        }

        // Public getters
        public float Health => _health;
        public float MaxHealth => _maxHealth;
        public bool IsDefeated => _isDefeated;
        public BossState CurrentState => _currentState;
        public float HealthPercent => _health / _maxHealth;
    }
}
