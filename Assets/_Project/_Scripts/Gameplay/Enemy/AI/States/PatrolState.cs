using UnityEngine;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Patrol state - enemy patrols area and looks for targets
    /// SOLID: Single Responsibility - Only handles patrol behavior
    /// </summary>
    public class PatrolState : EnemyStateBase {
        private float _targetCheckTimer;
        private readonly float _targetCheckInterval = 1f;
        private readonly float _patrolRadius = 5f;
        private Vector3 _patrolCenter;
        private float _patrolAngle;
        
        public PatrolState(IEnemyEntity enemyEntity, IEnemyAI aiController) 
            : base(enemyEntity, aiController, EnemyAIState.Patrol) {
        }
        
        protected override void OnEnter() {
            _targetCheckTimer = 0f;
            _patrolCenter = EnemyEntity.GameObject.transform.position;
            _patrolAngle = Random.Range(0f, Mathf.PI * 2f);
            Debug.Log($"[{EnemyEntity}] Starting patrol around {_patrolCenter}");
        }
        
        protected override void OnExit() {
            // Cleanup when leaving patrol state
        }
        
        protected override void OnUpdate() {
            // Patrol movement
            UpdatePatrolMovement();
            
            // Periodically check for targets
            _targetCheckTimer -= Time.deltaTime;
            if (_targetCheckTimer <= 0f) {
                CheckForTargets();
                _targetCheckTimer = _targetCheckInterval;
            }
            
            // Transition to chase if target found
            if (HasTarget()) {
                AIController.ChangeState(EnemyAIState.Chase);
            }
        }
        
        private void UpdatePatrolMovement() {
            // Circular patrol pattern
            _patrolAngle += Time.deltaTime * 0.5f; // Patrol speed
            
            Vector3 patrolPoint = _patrolCenter + new Vector3(
                Mathf.Cos(_patrolAngle) * _patrolRadius,
                0f,
                Mathf.Sin(_patrolAngle) * _patrolRadius
            );
            
            EnemyEntity.MoveTowards(patrolPoint);
        }
        
        private void CheckForTargets() {
            // Look for player in detection radius
            var player = FindPlayerInRadius();
            if (player != null) {
                EnemyEntity.SetTarget(player);
            }
        }
        
        private IDamageable FindPlayerInRadius() {
            var colliders = Physics.OverlapSphere(
                EnemyEntity.GameObject.transform.position, 
                EnemyEntity.DetectionRadius, 
                LayerMask.GetMask("Player")
            );
            
            foreach (var collider in colliders) {
                var damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive) {
                    return damageable;
                }
            }
            
            return null;
        }
    }
}
