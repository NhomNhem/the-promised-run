using UnityEngine;

namespace ThePromisedRun.Core.Interfaces {
    /// <summary>
    /// Interface for entities that can attack
    /// SOLID: Interface Segregation - Only combat-related functionality
    /// </summary>
    public interface IAttacker {
        /// <summary>
        /// Base attack damage
        /// </summary>
        float BaseDamage { get; set; }
        
        /// <summary>
        /// Attack range for the entity
        /// </summary>
        float AttackRange { get; set; }
        
        /// <summary>
        /// Current attack cooldown
        /// </summary>
        float AttackCooldown { get; }
        
        /// <summary>
        /// Whether the entity can currently attack
        /// </summary>
        bool CanAttack { get; }
        
        /// <summary>
        /// Perform an attack
        /// </summary>
        void Attack();
        
        /// <summary>
        /// Check if target is in attack range
        /// </summary>
        /// <param name="target">Target to check</param>
        /// <returns>True if target is in range</returns>
        bool IsTargetInRange(IDamageable target);
        
        /// <summary>
        /// Event fired when attack starts
        /// </summary>
        System.Action OnAttackStart { get; set; }
        
        /// <summary>
        /// Event fired when attack hits a target
        /// </summary>
        System.Action<IDamageable> OnAttackHit { get; set; }
        
        /// <summary>
        /// Event fired when attack ends
        /// </summary>
        System.Action OnAttackEnd { get; set; }
    }
}
