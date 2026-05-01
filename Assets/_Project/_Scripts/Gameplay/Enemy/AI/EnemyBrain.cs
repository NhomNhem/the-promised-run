using UnityEngine;
using UnityEngine.AI;
using ThePromisedRun.Core.FSM;
using ThePromisedRun.Core.FSM.Interfaces;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Enemy.AI.States;

namespace ThePromisedRun.Gameplay.Enemy.AI {
    /// <summary>
    /// EnemyBrain — replaces EnemyAIController.
    /// Uses the same StateMachine as PlayerController for consistency.
    /// Wires: Idle ↔ Patrol ↔ Chase ↔ Attack, Dead (any).
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyBrain : MonoBehaviour {
        [Header("Detection")]
        [SerializeField] private float _detectionRadius = 8f;
        [SerializeField] private float _loseTargetDistance = 12f;
        [SerializeField] private LayerMask _playerMask;

        [Header("Debug")]
        [SerializeField] private bool _debugMode = false;

        // ── Runtime state ──────────────────────────────────────────────
        private StateMachine _fsm;
        private Enemy        _enemy;
        private EnemyHealth  _health;
        private NavMeshAgent _nav;

        // States (public for predicate access)
        private EnemyIdleState      _idle;
        private EnemyPatrolState    _patrol;
        private EnemyChaseState     _chase;
        private EnemyAttackFSMState _attack;
        private EnemyDeadFSMState   _dead;
        private EnemyHitState       _hit;

        // Hit flag — set by EnemyHealth.OnDamaged, consumed once by WasHit() predicate
        private bool _wasHit;

        // ── Cached target ──────────────────────────────────────────────
        private IDamageable _target;
        public  IDamageable Target => _target;

        // ── Predicates ─────────────────────────────────────────────────
        private bool HasTarget()       => _target != null && _target.IsAlive;
        private bool TargetInRange()   => HasTarget() && DistToTarget() <= _enemy.AttackRange;
        private bool TargetTooFar()    => HasTarget() && DistToTarget() > _loseTargetDistance;
        private bool AttackDone()      => _attack.AttackComplete;
        private bool IsDead()          => _health != null && !_health.IsAlive;

        /// <summary>
        /// Consumes the _wasHit flag — returns true once per hit event, then resets.
        /// Only triggers Hit state when enemy is still alive (Dead transition takes priority).
        /// </summary>
        private bool WasHit() {
            bool v = _wasHit;
            _wasHit = false;
            return v && _health != null && _health.IsAlive;
        }

        private float DistToTarget() {
            if (_target == null) return float.MaxValue;
            return Vector3.Distance(transform.position,
                ((MonoBehaviour)_target).transform.position);
        }

        // ── Unity lifecycle ────────────────────────────────────────────
        private void Awake() {
            _enemy  = GetComponent<Enemy>();
            _health = GetComponent<EnemyHealth>();
            _nav    = GetComponent<NavMeshAgent>();

            // Default player mask: try "Player" layer, fall back to Default
            if (_playerMask.value == 0) {
                int playerLayer = LayerMask.NameToLayer("Player");
                _playerMask = playerLayer >= 0
                    ? (1 << playerLayer)
                    : (1 << 0); // Default layer fallback
            }

            // Subscribe to damage events so FSM can transition to Hit state
            if (_health != null) {
                _health.OnDamaged.AddListener(_ => {
                    if (_health.IsAlive) _wasHit = true;
                });
            }
        }

        private void Start() {
            // Delay FSM setup by one frame to ensure NavMeshAgent is fully initialized
            StartCoroutine(SetupFSMDelayed());
        }

        private System.Collections.IEnumerator SetupFSMDelayed() {
            // Wait for NavMeshAgent to be placed on NavMesh
            var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
            float timeout = 3f;
            while (nav != null && !nav.isOnNavMesh && timeout > 0f) {
                timeout -= UnityEngine.Time.deltaTime;
                yield return null;
            }
            SetupFSM();
        }

        private void Update() {
            ScanForPlayer();
            _fsm?.Update();
        }

        private void FixedUpdate() {
            _fsm?.FixedUpdate();
        }

        // ── FSM Setup ──────────────────────────────────────────────────
        private void SetupFSM() {
            var hitbox = GetComponentInChildren<EnemyAttackHitbox>();

            _idle   = new EnemyIdleState(_enemy);
            _patrol = new EnemyPatrolState(_enemy);
            _chase  = new EnemyChaseState(_enemy);
            _attack = new EnemyAttackFSMState(_enemy, hitbox);
            _dead   = new EnemyDeadFSMState(_enemy);
            _hit    = new EnemyHitState(_enemy);

            _fsm = new StateMachine();

            // ── Transitions ────────────────────────────────────────────
            // Idle → Patrol (immediately)
            _fsm.AddTransition(_idle, _patrol, new FuncPredicate(() => true));

            // Patrol ↔ Chase
            _fsm.AddTransition(_patrol, _chase,  new FuncPredicate(HasTarget));
            _fsm.AddTransition(_chase,  _patrol, new FuncPredicate(() => !HasTarget() || TargetTooFar()));

            // Chase → Attack (in range)
            _fsm.AddTransition(_chase,  _attack, new FuncPredicate(TargetInRange));

            // Attack → Chase (attack done, not in range)
            _fsm.AddTransition(_attack, _chase,  new FuncPredicate(() => AttackDone() && !TargetInRange()));
            // Attack → Attack (attack done, still in range — re-enter)
            _fsm.AddTransition(_attack, _chase,  new FuncPredicate(AttackDone));

            // Hit → Chase / Patrol (after stun duration)
            _fsm.AddTransition(_hit, _chase,  new FuncPredicate(() => _hit.HitComplete && HasTarget()));
            _fsm.AddTransition(_hit, _patrol, new FuncPredicate(() => _hit.HitComplete && !HasTarget()));

            // Any → Hit (when damaged and alive) — checked BEFORE Dead so it doesn't fire on killing blow
            _fsm.AddAnyTransition(_hit,  new FuncPredicate(WasHit));

            // Any → Dead (takes priority over Hit on killing blow because IsDead checks !IsAlive)
            _fsm.AddAnyTransition(_dead, new FuncPredicate(IsDead));

            // Start in Idle (→ Patrol immediately via predicate)
            _fsm.SetState(_idle);

            if (_debugMode) Debug.Log($"[EnemyBrain] FSM setup complete for {gameObject.name}");
        }

        // ── Player detection ───────────────────────────────────────────
        private void ScanForPlayer() {
            if (IsDead()) return;
            if (HasTarget()) return; // already tracking

            // OverlapSphere — works with any layer including Default
            var cols = Physics.OverlapSphere(transform.position, _detectionRadius, _playerMask);
            foreach (var col in cols) {
                var dmg = col.GetComponentInParent<IDamageable>();
                if (dmg != null && dmg.IsAlive) {
                    _target = dmg;
                    _enemy.SetTarget(dmg);
                    if (_debugMode) Debug.Log($"[EnemyBrain] Target acquired: {col.name}");
                    return;
                }
            }

            // Also check by tag as fallback
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null) {
                float dist = Vector3.Distance(transform.position, playerGO.transform.position);
                if (dist <= _detectionRadius) {
                    var dmg = playerGO.GetComponent<IDamageable>();
                    if (dmg != null && dmg.IsAlive) {
                        _target = dmg;
                        _enemy.SetTarget(dmg);
                    }
                }
            }
        }

        // ── Public API ─────────────────────────────────────────────────
        public void LoseTarget() {
            _target = null;
            _enemy.ClearTarget();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, _loseTargetDistance);
        }
    }
}
