using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay;
using ThePromisedRun.Gameplay.Enemy;

namespace ThePromisedRun.Core.Systems {
    /// <summary>
    /// Centralized damage system following SOLID principles
    /// SOLID: Single Responsibility - Only handles damage calculations and applications
    /// </summary>
    public static class DamageSystem {
        /// <summary>
        /// Apply damage to a target with proper damage calculation
        /// SOLID: Open/Closed - Can be extended for different damage types
        /// </summary>
        /// <param name="target">Target to damage</param>
        /// <param name="damageAmount">Base damage amount</param>
        /// <param name="attacker">Attacker entity</param>
        /// <param name="hitPoint">Where the hit occurred</param>
        /// <param name="hitNormal">Direction of the hit</param>
        /// <param name="damageType">Type of damage</param>
        public static void ApplyDamage(IDamageable target, float damageAmount, GameObject attacker, 
            Vector3 hitPoint, Vector3 hitNormal, DamageType damageType = DamageType.Physical) {
            
            if (target == null || !target.IsAlive) return;
            
            // Calculate final damage based on damage type
            float finalDamage = CalculateDamage(damageAmount, damageType, attacker, target);
            
            // Create damage info
            var damageInfo = new DamageInfo(
                finalDamage,
                hitPoint,
                hitNormal,
                attacker,
                damageType == DamageType.Overload
            );
            
            // Apply damage to target
            target.TakeDamage(finalDamage, damageInfo);
            
            Debug.Log($"[DamageSystem] Applied {finalDamage:F1} damage to {target.GetType().Name} from {attacker?.name}");
        }
        
        /// <summary>
        /// Calculate final damage based on various factors
        /// </summary>
        private static float CalculateDamage(float baseDamage, DamageType damageType, GameObject attacker, IDamageable target) {
            float finalDamage = baseDamage;
            
            // Apply damage type multipliers
            switch (damageType) {
                case DamageType.Physical:
                    finalDamage *= 1.0f;
                    break;
                case DamageType.Overload:
                    finalDamage *= 2.5f;
                    break;
                case DamageType.Magic:
                    finalDamage *= 1.2f;
                    break;
                case DamageType.Environmental:
                    finalDamage *= 0.8f;
                    break;
            }
            
            // Apply attacker multipliers
            if (attacker != null) {
                // Check if attacker has damage bonuses
                var attackerEntity = attacker.GetComponent<PlayerController>();
                if (attackerEntity != null && attackerEntity.IsOverloaded) {
                    finalDamage *= 3f;
                }
            }
            
            // Apply target resistances
            if (target != null) {
                // Example: Enemy type resistances
                var enemyTarget = target as Enemy;
                if (enemyTarget != null) {
                    // Enemy-specific resistances can be implemented here
                    finalDamage *= enemyTarget.GetDamageResistance(damageType);
                }
            }
            
            return Mathf.Max(0f, finalDamage);
        }
        
        /// <summary>
        /// Create area damage (explosion, AoE attacks)
        /// </summary>
        /// <param name="center">Center of damage</param>
        /// <param name="radius">Damage radius</param>
        /// <param name="damage">Base damage</param>
        /// <param name="attacker">Attacker entity</param>
        /// <param name="damageType">Type of damage</param>
        /// <param name="targetLayers">Layers to damage</param>
        public static void ApplyAreaDamage(Vector3 center, float radius, float damage, 
            GameObject attacker, DamageType damageType = DamageType.Physical, 
            LayerMask targetLayers = default) {
            
            Debug.Log($"[DamageSystem] Applying {damage} area damage at {center}");
            
            // Find all damageable targets in radius
            var colliders = Physics.OverlapSphere(center, radius, targetLayers);
            
            foreach (var collider in colliders) {
                var damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive) {
                    Vector3 hitPoint = collider.ClosestPoint(center);
                    Vector3 hitNormal = (center - hitPoint).normalized;
                    
                    ApplyDamage(damageable, damage, attacker, hitPoint, hitNormal, damageType);
                }
            }
        }
        
        /// <summary>
        /// Create directional damage (raycast attacks)
        /// </summary>
        /// <param name="origin">Attack origin</param>
        /// <param name="direction">Attack direction</param>
        /// <param name="range">Attack range</param>
        /// <param name="damage">Base damage</param>
        /// <param name="attacker">Attacker entity</param>
        /// <param name="damageType">Type of damage</param>
        /// <param name="targetLayers">Layers to check</param>
        public static void ApplyDirectionalDamage(Vector3 origin, Vector3 direction, float range, 
            float damage, GameObject attacker, DamageType damageType = DamageType.Physical, 
            LayerMask targetLayers = default) {
            
            Debug.Log($"[DamageSystem] Applying {damage} directional damage from {origin}");
            
            if (Physics.Raycast(origin, direction, out RaycastHit hit, range, targetLayers)) {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive) {
                    ApplyDamage(damageable, damage, attacker, hit.point, hit.normal, damageType);
                }
            }
        }
        
        /// <summary>
        /// Heal a target
        /// </summary>
        /// <param name="target">Target to heal</param>
        /// <param name="amount">Heal amount</param>
        public static void Heal(IDamageable target, float amount) {
            if (target == null || !target.IsAlive) return;
            
            // Check if target is an Entity to access Heal method
            if (target is Entity entity) {
                entity.Heal(amount);
                Debug.Log($"[DamageSystem] Healed {amount} health to {target.GetType().Name}");
            } else {
                Debug.LogWarning($"[DamageSystem] Cannot heal {target.GetType().Name} - not an Entity");
            }
        }
        
        /// <summary>
        /// Check if entity can be damaged
        /// </summary>
        /// <param name="target">Target to check</param>
        /// <param name="attacker">Potential attacker</param>
        /// <returns>True if target can be damaged</returns>
        public static bool CanDamage(IDamageable target, GameObject attacker = null) {
            if (target == null || !target.IsAlive) return false;
            
            // Check friendly fire (same team/layer)
            if (attacker != null) {
                var attackerEntity = attacker.GetComponent<Entity>();
                var targetEntity = target as Entity;
                
                // Don't damage entities of the same type (can be overridden for team systems)
                if (attackerEntity != null && targetEntity != null) {
                    if (attackerEntity.GetType() == targetEntity.GetType()) {
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// Types of damage
    /// </summary>
    public enum DamageType {
        Physical,
        Magic,
        Environmental,
        Overload,
        Poison,
        Fire,
        Ice,
        Lightning
    }
}
