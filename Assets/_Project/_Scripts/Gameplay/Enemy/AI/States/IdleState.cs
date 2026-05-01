using UnityEngine;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Idle state - enemy waits and looks for targets
    /// SOLID: Single Responsibility - Only handles idle behavior
    /// </summary>
    public class IdleState : EnemyStateBase {
        private float _targetCheckTimer;
        private readonly float _targetCheckInterval = 0.5f;
        
        public IdleState(IEnemyEntity enemyEntity, IEnemyAI aiController) 
            : base(enemyEntity, aiController, EnemyAIState.Idle) {
        }
        
        protected override void OnEnter() {
            _targetCheckTimer = 0f;
            EnemyEntity.StopMovement();
        }
        
        protected override void OnExit() {
            // Cleanup when leaving idle state
        }
        
        protected override void OnUpdate() {
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
        
        private void CheckForTargets() {
            // Look for player in detection radius
            var player = FindPlayerInRadius();
            if (player != null) {
                AIController.SetTarget(player);
            }
        }
        
        private IDamageable FindPlayerInRadius() {
            // Simple overlap sphere detection
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
