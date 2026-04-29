using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Chase state - enemy pursues target
    /// SOLID: Single Responsibility - Only handles chase behavior
    /// </summary>
    public class ChaseState : EnemyStateBase {
        private float _attackDecisionTimer;
        private readonly float _attackDecisionInterval = 0.2f;
        private readonly float _loseTargetTime = 5f;
        private Animator _anim; // cached once in OnEnter
        
        public ChaseState(IEnemyEntity enemyEntity, IEnemyAI aiController) 
            : base(enemyEntity, aiController, EnemyAIState.Chase) {
        }
        
        protected override void OnEnter() {
            _attackDecisionTimer = 0f;
            _anim = EnemyEntity.GameObject.GetComponentInChildren<Animator>(); // cache once
            _anim?.SetBool("IsMoving", true);
            Debug.Log($"[{EnemyEntity}] Starting chase of target");
        }
        
        protected override void OnExit() {
            // Cleanup when leaving chase state
        }
        
        protected override void OnUpdate() {
            if (!HasTarget()) {
                _anim?.SetBool("IsMoving", false);
                AIController.ChangeState(EnemyAIState.Idle);
                return;
            }

            EnemyEntity.MoveTowards(EnemyEntity.LastKnownTargetPosition);
            EnemyEntity.FaceTarget(EnemyEntity.CurrentTarget);

            // Drive walk animation based on actual velocity
            var nav = EnemyEntity.GameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (_anim != null && nav != null)
                _anim.SetBool("IsMoving", nav.velocity.sqrMagnitude > 0.1f);
            
            // Check if should attack
            _attackDecisionTimer -= Time.deltaTime;
            if (_attackDecisionTimer <= 0f && CanAttack()) {
                AIController.ChangeState(EnemyAIState.Attack);
                _attackDecisionTimer = _attackDecisionInterval;
            }
            
            // Check if lost target
            if (EnemyEntity.TimeSinceLastSeenTarget >= _loseTargetTime) {
                AIController.ChangeState(EnemyAIState.Idle);
            }
        }
        
        public override void OnDamaged(DamageInfo damageInfo) {
            // When damaged in chase state, briefly check if should attack immediately
            if (CanAttack()) {
                AIController.ChangeState(EnemyAIState.Attack);
            }
        }
    }
}
