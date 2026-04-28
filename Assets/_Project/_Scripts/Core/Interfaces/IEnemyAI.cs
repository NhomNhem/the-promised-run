using UnityEngine;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Core.Interfaces {
    /// <summary>
    /// Interface for Enemy AI behavior
    /// SOLID: Interface Segregation - Only AI-related functionality
    /// SOLID: Dependency Inversion - High-level modules depend on abstractions
    /// </summary>
    public interface IEnemyAI {
        /// <summary>
        /// Current AI state
        /// </summary>
        EnemyAIState CurrentState { get; }
        
        /// <summary>
        /// Whether AI is currently active
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Initialize AI with target reference
        /// </summary>
        /// <param name="enemy">The enemy entity</param>
        void Initialize(IEnemyEntity enemy);
        
        /// <summary>
        /// Update AI behavior
        /// </summary>
        void UpdateAI();
        
        /// <summary>
        /// Change AI state
        /// </summary>
        /// <param name="newState">New state to transition to</param>
        void ChangeState(EnemyAIState newState);
        
        /// <summary>
        /// Set target for AI to track
        /// </summary>
        /// <param name="target">Target entity</param>
        void SetTarget(IDamageable target);
        
        /// <summary>
        /// Clear current target
        /// </summary>
        void ClearTarget();
        
        /// <summary>
        /// Events for state changes
        /// </summary>
        System.Action<EnemyAIState> OnStateChanged { get; set; }
        System.Action<IDamageable> OnTargetAcquired { get; set; }
        System.Action OnTargetLost { get; set; }
    }
    
    /// <summary>
    /// Enemy AI states
    /// SOLID: Closed for modification, open for extension
    /// </summary>
    public enum EnemyAIState {
        Idle,
        Chase,
        Attack,
        Patrol,
        Dead,
        Stunned
    }
    
    /// <summary>
    /// Interface for Enemy entity (separated from AI concerns)
    /// SOLID: Interface Segregation - Only entity-specific functionality
    /// </summary>
    public interface IEnemyEntity : IDamageable {
        /// <summary>
        /// Movement capabilities
        /// </summary>
        float MoveSpeed { get; }
        float RotationSpeed { get; }
        
        /// <summary>
        /// Combat capabilities
        /// </summary>
        float AttackRange { get; }
        float BaseDamage { get; }
        bool CanAttack { get; }
        
        /// <summary>
        /// Detection capabilities
        /// </summary>
        float DetectionRadius { get; }
        
        /// <summary>
        /// Current target
        /// </summary>
        IDamageable CurrentTarget { get; }
        
        /// <summary>
        /// GameObject reference (for Unity-specific operations)
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// Movement methods
        /// </summary>
        void MoveTowards(Vector3 position);
        void StopMovement();
        void FaceTarget(IDamageable target);
        
        /// <summary>
        /// Combat methods
        /// </summary>
        void Attack();
        
        /// <summary>
        /// Target management
        /// </summary>
        bool HasTarget { get; }
        Vector3 LastKnownTargetPosition { get; }
        float TimeSinceLastSeenTarget { get; }
        
        /// <summary>
        /// Target management methods
        /// </summary>
        void SetTarget(IDamageable target);
        void ClearTarget();
        
        /// <summary>
        /// Range checking
        /// </summary>
        bool IsTargetInRange(IDamageable target);
    }
}
