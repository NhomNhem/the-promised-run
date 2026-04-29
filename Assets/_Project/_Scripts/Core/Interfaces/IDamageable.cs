using UnityEngine;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Core.Interfaces {
    /// <summary>
    /// Interface for entities that can take damage
    /// SOLID: Interface Segregation - Only damage-related functionality
    /// </summary>
    public interface IDamageable {
        /// <summary>
        /// Current health of the entity
        /// </summary>
        float Health { get; }
        
        /// <summary>
        /// Whether the entity is alive
        /// </summary>
        bool IsAlive { get; }
        
        /// <summary>
        /// Maximum health of the entity
        /// </summary>
        float MaxHealth { get; }
        
        /// <summary>
        /// Apply damage to the entity
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        /// <param name="info">Damage information including source and direction</param>
        void TakeDamage(float amount, DamageInfo info);
        
        /// <summary>
        /// Event fired when health changes
        /// </summary>
        System.Action<float> OnHealthChanged { get; set; }
        
        /// <summary>
        /// Event fired when entity dies
        /// </summary>
        System.Action OnDeath { get; set; }
    }
}
