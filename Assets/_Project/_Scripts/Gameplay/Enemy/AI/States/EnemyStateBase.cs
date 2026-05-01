using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Base class for Enemy AI states
    /// SOLID: Template Method Pattern - Defines state behavior structure
    /// SOLID: Open/Closed - Can be extended for new states without modifying base
    /// </summary>
    public abstract class EnemyStateBase {
        protected IEnemyEntity EnemyEntity;
        protected IEnemyAI AIController;
        
        public EnemyAIState StateType { get; protected set; }
        
        public EnemyStateBase(IEnemyEntity enemyEntity, IEnemyAI aiController, EnemyAIState stateType) {
            EnemyEntity = enemyEntity;
            AIController = aiController;
            StateType = stateType;
        }
        
        /// <summary>
        /// Called when entering this state
        /// </summary>
        public virtual void Enter() {
            OnEnter();
        }
        
        /// <summary>
        /// Called when exiting this state
        /// </summary>
        public virtual void Exit() {
            OnExit();
        }
        
        /// <summary>
        /// Update state behavior
        /// </summary>
        public virtual void Update() {
            if (!EnemyEntity.IsAlive) {
                AIController.ChangeState(EnemyAIState.Dead);
                return;
            }
            
            OnUpdate();
        }
        
        /// <summary>
        /// Called when taking damage
        /// </summary>
        public virtual void OnDamaged(DamageInfo damageInfo) {
            // Default behavior - can be overridden
        }
        
        // Abstract methods to be implemented by derived states
        protected abstract void OnEnter();
        protected abstract void OnExit();
        protected abstract void OnUpdate();
        
        // Helper methods
        protected bool HasTarget() => EnemyEntity.HasTarget;
        protected bool CanAttack() => EnemyEntity.CanAttack;
        protected bool TargetInRange() => HasTarget() && EnemyEntity.IsTargetInRange(EnemyEntity.CurrentTarget);
        protected float DistanceToTarget() => Vector3.Distance(EnemyEntity.GameObject.transform.position, EnemyEntity.LastKnownTargetPosition);
    }
}
