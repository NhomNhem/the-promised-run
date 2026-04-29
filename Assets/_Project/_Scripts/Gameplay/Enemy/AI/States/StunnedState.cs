using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Stunned state - enemy is temporarily disabled
    /// SOLID: Single Responsibility - Only handles stun behavior
    /// </summary>
    public class StunnedState : EnemyStateBase {
        private float _stunTimer;
        private readonly float _stunDuration = 2f;
        
        public StunnedState(IEnemyEntity enemyEntity, IEnemyAI aiController) 
            : base(enemyEntity, aiController, EnemyAIState.Stunned) {
        }
        
        protected override void OnEnter() {
            _stunTimer = _stunDuration;
            EnemyEntity.StopMovement();
            Debug.Log($"[{EnemyEntity}] Entering stunned state for {_stunDuration}s");
        }
        
        protected override void OnExit() {
            Debug.Log($"[{EnemyEntity}] Recovering from stun");
        }
        
        protected override void OnUpdate() {
            _stunTimer -= Time.deltaTime;
            
            if (_stunTimer <= 0f) {
                // Return to previous state or default to idle
                if (HasTarget()) {
                    AIController.ChangeState(EnemyAIState.Chase);
                } else {
                    AIController.ChangeState(EnemyAIState.Idle);
                }
            }
        }
        
        public override void OnDamaged(DamageInfo damageInfo) {
            // Extend stun if damage is overload type
            if (damageInfo.IsOverloadBoosted) {
                _stunTimer = _stunDuration;
                Debug.Log($"[{EnemyEntity}] Stun extended by overload damage");
            }
        }
    }
}
