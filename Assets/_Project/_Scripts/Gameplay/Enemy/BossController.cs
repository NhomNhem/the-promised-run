using UnityEngine;
using ThePromisedRun.UI;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// BossController — "The Architect" final boss.
    /// GDD §7: System Echo — invincible outside Overload.
    ///
    /// Phases:
    ///   Phase 1 (100–51% HP): Invincible. Player must trigger Overload to damage.
    ///   Phase 2 (50–26% HP):  Popup barrage every 1s (3 popups/burst). Still invincible outside Overload.
    ///   Phase 3 (25–0% HP):   Invincible again. Must Overload a second time to finish.
    ///
    /// Win condition: 3 hits during any Overload window OR HP reaches 0.
    /// On defeat: calls EndingSequenceController.PlayEnding().
    ///
    /// Visual states:
    ///   Invincible: red glow, scale 1.0
    ///   Vulnerable: white/bright, scale 1.1 (slightly larger = more threatening)
    ///   Hit:        white flash for 0.1s
    /// </summary>
    public class BossController : MonoBehaviour {
        [Header("Stats")]
        [SerializeField] private int   _maxHealth            = 100;
        [SerializeField] private bool  _useFixedHitsToKillForTesting = true;
        [SerializeField] private int   _hitsToKill = 5;

        [Header("Phase 2 Barrage")]
        [SerializeField] private float _popupBarrageInterval = 1f;
        [SerializeField] private int   _popupsPerBurst       = 3;

        [Header("References")]
        [SerializeField] private OverloadSystem          _overloadSystem;
        [SerializeField] private EndingSequenceController _endingController;
        [SerializeField] private PopupSpawner             _popupSpawner;

        [Header("Visual")]
        [SerializeField] private Animator  _animator;
        [SerializeField] private Renderer  _renderer;
        [SerializeField] private string[] _idleStateCandidates = { "Idle_A", "Melee_2H_Idle", "Melee_Unarmed_Idle", "Idle" };
        [SerializeField] private string[] _hitTriggerCandidates = { "Hit", "GetHit" };
        [SerializeField] private string[] _deathTriggerCandidates = { "Death", "Die" };

        private int   _health;
        private int   _remainingHits;
        private float _popupTimer;
        private bool  _isDefeated;
        private bool  _wasOverloaded;

        // Visual state
        private Material _material;
        private Color    _colorInvincible = new Color(1f, 0.1f, 0.1f);   // red
        private Color    _colorVulnerable = new Color(1f, 1f, 1f);        // white
        private Color    _colorHit        = new Color(1f, 1f, 0.5f);      // yellow flash
        private float    _hitFlashTimer;
        private const float HitFlashDuration = 0.1f;

        private void Start() {
            _health = _maxHealth;
            _remainingHits = Mathf.Max(1, _hitsToKill);

            if (_overloadSystem   == null) _overloadSystem   = FindFirstObjectByType<OverloadSystem>();
            if (_endingController == null) _endingController = FindFirstObjectByType<EndingSequenceController>();
            if (_popupSpawner     == null) _popupSpawner     = FindFirstObjectByType<PopupSpawner>();
            if (_animator         == null) _animator         = GetComponentInChildren<Animator>();
            if (_renderer         == null) _renderer         = GetComponentInChildren<Renderer>();

            // Create instance material for color changes
            if (_renderer != null) {
                _material = _renderer.material;
                SetVisualState(false); // start invincible
            }

            if (_overloadSystem == null)
                Debug.LogError("[BossController] OverloadSystem not found!");

            TryPlayIdleLarge();
        }

        private void Update() {
            if (_isDefeated || _overloadSystem == null) return;

            // Detect overload state change for visual update
            bool isOverloaded = _overloadSystem.IsActive;
            if (isOverloaded != _wasOverloaded) {
                _wasOverloaded = isOverloaded;
                SetVisualState(isOverloaded);
            }

            // Phase 2: popup barrage
            if (_health <= 50 && _health > 25) {
                _popupTimer += Time.deltaTime;
                if (_popupTimer >= _popupBarrageInterval) {
                    _popupTimer = 0f;
                    SpawnBarrage(_popupsPerBurst);
                }
            }

            // Hit flash countdown
            if (_hitFlashTimer > 0f) {
                _hitFlashTimer -= Time.deltaTime;
                if (_hitFlashTimer <= 0f && _material != null)
                    _material.color = isOverloaded ? _colorVulnerable : _colorInvincible;
            }
        }

        /// <summary>Called by player attack hitbox when hitting the boss.</summary>
        public void TakeDamage(int damage) {
            if (_isDefeated) return;

            if (_overloadSystem == null || !_overloadSystem.IsActive) {
                Debug.Log("[Boss] Invincible — trigger Overload to damage.");
                // Blocked visual: brief scale punch
                StartCoroutine(BlockedPunch());
                return;
            }

            if (_useFixedHitsToKillForTesting) {
                _remainingHits = Mathf.Max(0, _remainingHits - 1);
                _health = Mathf.RoundToInt(_maxHealth * ((float)_remainingHits / Mathf.Max(1, _hitsToKill)));
            } else {
                _health -= damage;
                _health  = Mathf.Max(0, _health);
            }
            DamagePopupSpawner.Spawn(transform.position, damage, DamagePopupType.Boss);

            Debug.Log($"[Boss] Hit! HP: {_health}/{_maxHealth}");

            // Hit flash
            if (_material != null) {
                _material.color = _colorHit;
                _hitFlashTimer  = HitFlashDuration;
            }
            TriggerFirstValid(_hitTriggerCandidates);

            if (_health <= 0)
                DefeatBoss();
        }

        public void TakeDamage(float damage) => TakeDamage(Mathf.RoundToInt(damage));

        private void SetVisualState(bool vulnerable) {
            if (_material == null) return;
            _material.color = vulnerable ? _colorVulnerable : _colorInvincible;

            // Scale: vulnerable = slightly larger (more threatening when exposed)
            transform.localScale = vulnerable
                ? new Vector3(1.5f, 2.2f, 1.5f)
                : new Vector3(1.5f, 2.0f, 1.5f);
        }

        private System.Collections.IEnumerator BlockedPunch() {
            Vector3 original = transform.localScale;
            transform.localScale = original * 0.9f;
            yield return new WaitForSeconds(0.05f);
            transform.localScale = original;
        }

        private void SpawnBarrage(int count) {
            if (_popupSpawner == null) return;
            for (int i = 0; i < count; i++)
                _popupSpawner.ForceSpawnPopup();
        }

        private void DefeatBoss() {
            if (_isDefeated) return;
            _isDefeated = true;

            Debug.Log("[Boss] DEFEATED. Triggering ending sequence...");
            TriggerFirstValid(_deathTriggerCandidates);

            if (_material != null)
                _material.color = Color.black; // goes dark on death

            if (_endingController != null)
                _endingController.PlayEnding();
            else
                Debug.LogError("[Boss] EndingSequenceController not found!");
        }

        public void SetHealthForTesting(int hp) => _health = Mathf.Clamp(hp, 0, _maxHealth);
        public int  Health     => _health;
        public bool IsDefeated => _isDefeated;

        private void TryPlayIdleLarge() {
            if (_animator == null || _idleStateCandidates == null) return;

            for (int i = 0; i < _idleStateCandidates.Length; i++) {
                string stateName = _idleStateCandidates[i];
                if (string.IsNullOrWhiteSpace(stateName)) continue;
                int stateHash = Animator.StringToHash(stateName);
                if (_animator.HasState(0, stateHash)) {
                    _animator.CrossFade(stateHash, 0.1f, 0);
                    return;
                }
            }
        }

        private void TriggerFirstValid(string[] triggerCandidates) {
            if (_animator == null || triggerCandidates == null) return;

            for (int i = 0; i < triggerCandidates.Length; i++) {
                string trigger = triggerCandidates[i];
                if (string.IsNullOrWhiteSpace(trigger)) continue;
                _animator.SetTrigger(trigger);
                return;
            }
        }
    }
}
