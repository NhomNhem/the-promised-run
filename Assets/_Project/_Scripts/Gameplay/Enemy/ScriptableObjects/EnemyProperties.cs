using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Core.Systems;

namespace ThePromisedRun.Gameplay.Enemy.ScriptableObjects {
    /// <summary>
    /// ScriptableObject containing all enemy properties and stats
    /// Designed for use with Odin Inspector for easy configuration and balancing
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyProperties", menuName = "The Promised Run/Enemy/Enemy Properties")]
    public class EnemyProperties : ScriptableObject {
        [Header("Combat Stats")]
        [Min(1f)]
        public float baseDamage = 15f;
        
        [Min(0.1f)]
        public float attackRange = 2f;
        
        [Min(0.1f)]
        public float attackCooldown = 1f;
        
        [Min(0.1f)]
        [Range(0.1f, 1f)]
        public float attackWindupTime = 0.3f;
        
        [Min(0.1f)]
        [Range(0.1f, 2f)]
        public float attackRecoveryTime = 0.5f;
        
        [Header("Movement Stats")]
        [Min(0.1f)]
        public float moveSpeed = 5f;
        
        [Min(0.1f)]
        public float rotationSpeed = 120f;
        
        [Min(0.1f)]
        public float acceleration = 8f;
        
        [Min(0.1f)]
        public float deceleration = 10f;
        
        [Min(0.1f)]
        public float maxSpeed = 8f;
        
        [Header("Health Stats")]
        [Min(10f)]
        public float maxHealth = 50f;
        
        [Min(0f)]
        public float healthRegenRate = 0f;
        
        [Min(0f)]
        public float healthRegenDelay = 5f;
        
        [Header("Defense Stats")]
        [Range(0f, 1f)]
        public float damageResistance = 0f;
        
        [Range(0f, 1f)]
        public float knockbackResistance = 0f;
        
        [Range(0f, 1f)]
        public float stunResistance = 0f;
        
        [Header("Detection Stats")]
        [Min(0.1f)]
        public float detectionRadius = 10f;
        
        [Min(0f)]
        [Range(0f, 360f)]
        public float detectionAngle = 180f;
        
        [Min(0f)]
        public float targetUpdateInterval = 0.5f;
        
        [Min(0f)]
        public float loseTargetTime = 3f;
        
        [Header("AI Behavior")]
        [Min(0f)]
        public float wanderRadius = 5f;
        
        [Min(0f)]
        public float patrolRadius = 5f;
        
        [Min(0f)]
        public float patrolSpeed = 3f;
        
        [Min(0f)]
        public float patrolWaitTime = 1f;
        
        [Min(0f)]
        public float chaseSpeed = 8f;
        
        [Min(0f)]
        public float chaseUpdateInterval = 0.3f;
        
        [Header("Juice Settings")]
        [Min(0f)]
        public float hitStunDuration = 0.2f;
        
        [Min(0f)]
        public float deathDuration = 2f;
        
        [Min(0f)]
        public float hitFlashDuration = 0.1f;
        
        [Min(0f)]
        public float deathFadeDuration = 1f;
        
        [Min(0.5f)]
        public float attackEffectScale = 1.1f;
        
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float footstepVolume = 0.3f;
        
        [Range(0f, 1f)]
        public float attackVolume = 0.7f;
        
        [Range(0f, 1f)]
        public float hurtVolume = 0.8f;
        
        [Range(0f, 1f)]
        public float deathVolume = 0.9f;
        
        [Header("Debug Settings")]
        public bool showDebugInfo = false;
        public bool showDetectionGizmos = false;
        public bool showPatrolPath = false;
        public bool showAttackRange = false;
        
        public void ValidateSettings() {
            baseDamage = Mathf.Max(1f, baseDamage);
            attackRange = Mathf.Max(0.1f, attackRange);
            attackCooldown = Mathf.Max(0.1f, attackCooldown);
            attackWindupTime = Mathf.Max(0.1f, attackWindupTime);
            attackRecoveryTime = Mathf.Max(0.1f, attackRecoveryTime);
            
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            rotationSpeed = Mathf.Max(0.1f, rotationSpeed);
            acceleration = Mathf.Max(0.1f, acceleration);
            deceleration = Mathf.Max(0.1f, deceleration);
            maxSpeed = Mathf.Max(0.1f, maxSpeed);
            
            maxHealth = Mathf.Max(10f, maxHealth);
            healthRegenRate = Mathf.Max(0f, healthRegenRate);
            healthRegenDelay = Mathf.Max(0f, healthRegenDelay);
            
            damageResistance = Mathf.Clamp01(damageResistance);
            knockbackResistance = Mathf.Clamp01(knockbackResistance);
            stunResistance = Mathf.Clamp01(stunResistance);
            
            detectionRadius = Mathf.Max(0.1f, detectionRadius);
            detectionAngle = Mathf.Clamp(detectionAngle, 0f, 360f);
            targetUpdateInterval = Mathf.Max(0f, targetUpdateInterval);
            loseTargetTime = Mathf.Max(0f, loseTargetTime);
            
            wanderRadius = Mathf.Max(0f, wanderRadius);
            patrolSpeed = Mathf.Max(0f, patrolSpeed);
            patrolWaitTime = Mathf.Max(0f, patrolWaitTime);
            chaseSpeed = Mathf.Max(0f, chaseSpeed);
            chaseUpdateInterval = Mathf.Max(0f, chaseUpdateInterval);
            
            hitStunDuration = Mathf.Max(0f, hitStunDuration);
            deathDuration = Mathf.Max(0f, deathDuration);
            hitFlashDuration = Mathf.Max(0f, hitFlashDuration);
            deathFadeDuration = Mathf.Max(0f, deathFadeDuration);
            attackEffectScale = Mathf.Max(0.5f, attackEffectScale);
            
            footstepVolume = Mathf.Clamp01(footstepVolume);
            attackVolume = Mathf.Clamp01(attackVolume);
            hurtVolume = Mathf.Clamp01(hurtVolume);
            deathVolume = Mathf.Clamp01(deathVolume);
        }
        
        private void OnValidate() {
            ValidateSettings();
        }
        
        public EnemyProperties Clone() {
            var clone = CreateInstance<EnemyProperties>();
            clone.baseDamage = baseDamage;
            clone.attackRange = attackRange;
            clone.attackCooldown = attackCooldown;
            clone.attackWindupTime = attackWindupTime;
            clone.attackRecoveryTime = attackRecoveryTime;
            clone.moveSpeed = moveSpeed;
            clone.rotationSpeed = rotationSpeed;
            clone.acceleration = acceleration;
            clone.deceleration = deceleration;
            clone.maxSpeed = maxSpeed;
            clone.maxHealth = maxHealth;
            clone.healthRegenRate = healthRegenRate;
            clone.healthRegenDelay = healthRegenDelay;
            clone.damageResistance = damageResistance;
            clone.knockbackResistance = knockbackResistance;
            clone.stunResistance = stunResistance;
            clone.detectionRadius = detectionRadius;
            clone.detectionAngle = detectionAngle;
            clone.targetUpdateInterval = targetUpdateInterval;
            clone.loseTargetTime = loseTargetTime;
            clone.wanderRadius = wanderRadius;
            clone.patrolSpeed = patrolSpeed;
            clone.patrolWaitTime = patrolWaitTime;
            clone.chaseSpeed = chaseSpeed;
            clone.chaseUpdateInterval = chaseUpdateInterval;
            clone.hitStunDuration = hitStunDuration;
            clone.deathDuration = deathDuration;
            clone.hitFlashDuration = hitFlashDuration;
            clone.deathFadeDuration = deathFadeDuration;
            clone.attackEffectScale = attackEffectScale;
            clone.footstepVolume = footstepVolume;
            clone.attackVolume = attackVolume;
            clone.hurtVolume = hurtVolume;
            clone.deathVolume = deathVolume;
            clone.showDebugInfo = showDebugInfo;
            clone.showDetectionGizmos = showDetectionGizmos;
            clone.showPatrolPath = showPatrolPath;
            clone.showAttackRange = showAttackRange;
            return clone;
        }
    }
}