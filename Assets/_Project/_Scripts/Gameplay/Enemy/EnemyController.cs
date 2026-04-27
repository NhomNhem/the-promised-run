using UnityEngine;
using System.Linq;
using ThePromisedRun.Core.FSM;
using ThePromisedRun.Gameplay.Combat;
using UnityEngine.Events;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Base enemy controller. Manages FSM, health, and player detection.
    /// Skeleton (and future enemies) extend this via config, not inheritance.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour, IDamageable {
        #region Inspector
        [Header("Stats")]
        [SerializeField] private float maxHealth    = 30f;
        [SerializeField] private float moveSpeed    = 3.5f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackRange  = 1.2f;
        [SerializeField] private float aggroRadius  = 5f;

        [Header("Overload Stun")]
        [SerializeField] private float stunDuration = 2f;

        [Header("References")]
        [SerializeField] private Transform visual;
        [SerializeField] private float rotationSpeed = 8f;
        #endregion

        #region Public Properties
        public Rigidbody  Rb           { get; private set; }
        public Animator   Anim         { get; private set; }
        public Transform  Target       { get; private set; }
        public float      MoveSpeed    => moveSpeed;
        public float      AttackDamage => attackDamage;
        public float      AttackRange  => attackRange;
        public float      AggroRadius  => aggroRadius;
        public float      StunDuration => stunDuration;
        public bool       IsAlive      => _health > 0f;
        public bool       IsStunned    => _stunTimer > 0f;
        public Transform  Visual       => visual;
        public float      RotationSpeed => rotationSpeed;
        #endregion

        #region Events
        public UnityEvent OnDeath   = new UnityEvent();
        public UnityEvent OnHit     = new UnityEvent();
        #endregion

        #region Private
        private StateMachine _fsm;
        private float        _health;
        private float        _stunTimer;
        #endregion

        private void Awake() {
            Rb   = GetComponent<Rigidbody>();
            Anim = visual != null
                ? visual.GetComponent<Animator>()
                : GetComponentInChildren<Animator>();

            _health = maxHealth;

            // Find player
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) Target = player.transform;

            _fsm = new StateMachine();
            SetupFSM();
        }

        private void SetupFSM() {
            var idle   = new EnemyIdleState(this, Anim);
            var chase  = new EnemyChaseState(this, Anim);
            var attack = new EnemyAttackState(this, Anim);
            var death  = new EnemyDeathState(this, Anim);

            _fsm.AddTransition(idle,   chase,  new FuncPredicate(() => IsAlive && !IsStunned && PlayerInAggroRange()));
            _fsm.AddTransition(chase,  idle,   new FuncPredicate(() => IsAlive && !IsStunned && !PlayerInAggroRange()));
            _fsm.AddTransition(chase,  attack, new FuncPredicate(() => IsAlive && !IsStunned && PlayerInAttackRange()));
            _fsm.AddTransition(attack, chase,  new FuncPredicate(() => IsAlive && !IsStunned && !PlayerInAttackRange() && attack.IsComplete));
            _fsm.AddTransition(attack, idle,   new FuncPredicate(() => IsAlive && !IsStunned && !PlayerInAggroRange() && attack.IsComplete));
            _fsm.AddAnyTransition(death, new FuncPredicate(() => !IsAlive));

            _fsm.SetState(idle);
        }

        private void Update() {
            if (_stunTimer > 0f) {
                _stunTimer -= Time.deltaTime;
                Rb.linearVelocity = Vector3.zero;
                return;
            }
            _fsm.Update();
        }

        private void FixedUpdate() {
            if (_stunTimer > 0f) return;
            _fsm.FixedUpdate();
        }

        public bool PlayerInAggroRange() =>
            Target != null && Vector3.Distance(transform.position, Target.position) <= aggroRadius;

        public bool PlayerInAttackRange() =>
            Target != null && Vector3.Distance(transform.position, Target.position) <= attackRange;

        public void FaceTarget() {
            if (Target == null || visual == null) return;
            Vector3 dir = (Target.position - transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            visual.rotation = Quaternion.Slerp(
                visual.rotation,
                Quaternion.LookRotation(dir),
                rotationSpeed * Time.deltaTime);
        }

        #region IDamageable
        public void TakeDamage(float amount, DamageInfo info) {
            if (!IsAlive) return;

            _health = Mathf.Max(0f, _health - amount);

            if (info.IsOverloadBoosted)
                _stunTimer = stunDuration;

            if (!IsAlive) {
                OnDeath.Invoke();
            } else {
                Anim.SetTrigger("GetHit");
                OnHit.Invoke();
            }
        }
        #endregion
    }
}
