using UnityEngine;
using UnityEngine.Events;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Core {
    /// <summary>
    /// Base entity class following SOLID principles
    /// SOLID: Single Responsibility - Manages only entity state and health
    /// </summary>
    public abstract class Entity : MonoBehaviour, IDamageable {
        [Header("Entity Properties")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected bool isAlive = true;
        
        // IDamageable Implementation
        public virtual float Health => currentHealth;
        public virtual bool IsAlive => isAlive;
        public virtual float MaxHealth => maxHealth;
        
        public System.Action<float> OnHealthChanged { get; set; }
        public System.Action OnDeath { get; set; }
        
        protected virtual void Awake() {
            currentHealth = maxHealth;
            isAlive = true;
            
            // Initialize events
            OnHealthChanged = (health) => { };
            OnDeath = () => { };
        }
        
        /// <summary>
        /// Apply damage to the entity
        /// SOLID: Open/Closed - Can be extended by derived classes
        /// </summary>
        public virtual void TakeDamage(float amount, DamageInfo info) {
            if (!IsAlive) return;
            
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth);
            OnHealthChanged.Invoke(currentHealth / maxHealth);
            
            if (currentHealth <= 0f) {
                Die();
            }
        }
        
        /// <summary>
        /// Heal the entity
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public virtual void Heal(float amount) {
            if (!IsAlive) return;
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth);
            OnHealthChanged.Invoke(currentHealth / maxHealth);
        }
        
        /// <summary>
        /// Kill the entity
        /// </summary>
        public virtual void Die() {
            if (!IsAlive) return;
            
            isAlive = false;
            currentHealth = 0f;
            
            OnDeath?.Invoke();
            OnHealthChanged.Invoke(0f);
            
            // Call derived class death logic
            OnDeath();
        }
        
        /// <summary>
        /// Revive the entity
        /// </summary>
        /// <param name="healthPercentage">Health percentage (0-1)</param>
        public virtual void Revive(float healthPercentage = 1f) {
            if (IsAlive) return;
            
            isAlive = true;
            currentHealth = Mathf.Max(1f, maxHealth * healthPercentage);
            
            OnHealthChanged?.Invoke(currentHealth);
            OnHealthChanged.Invoke(currentHealth / maxHealth);
            
            // Call derived class revive logic
            OnRevive();
        }
        
        /// <summary>
        /// Called when entity dies - override in derived classes
        /// </summary>
        protected abstract void HandleDeath();
        
        /// <summary>
        /// Called when entity revives - override in derived classes
        /// </summary>
        protected virtual void OnRevive() { }
        
        /// <summary>
        /// Reset entity to initial state
        /// </summary>
        public virtual void Reset() {
            currentHealth = maxHealth;
            isAlive = true;
            OnHealthChanged.Invoke(currentHealth);
            OnHealthChanged.Invoke(1f);
        }
    }
}
