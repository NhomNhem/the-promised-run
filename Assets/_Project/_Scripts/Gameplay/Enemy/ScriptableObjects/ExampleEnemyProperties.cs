using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Core.Systems;

namespace ThePromisedRun.Gameplay.Enemy.ScriptableObjects {
    /// <summary>
    /// Example Enemy Properties configurations for different enemy types
    /// These demonstrate how to create balanced enemy configurations using Scriptable Objects
    /// </summary>
    
    /// <summary>
    /// Standard Enemy Properties - balanced melee enemy
    /// </summary>
    [CreateAssetMenu(fileName = "StandardEnemyProperties", menuName = "The Promised Run/Enemy/Standard Enemy Properties")]
    public class StandardEnemyProperties : EnemyProperties {
        private void OnEnable() {
            // Combat Stats - balanced melee combat
            baseDamage = 15f;
            attackRange = 2f;
            attackCooldown = 1f;
            attackWindupTime = 0.3f;
            attackRecoveryTime = 0.5f;
            
            // Movement Stats - standard movement
            moveSpeed = 5f;
            rotationSpeed = 120f;
            acceleration = 8f;
            deceleration = 10f;
            maxSpeed = 8f;
            
            // Health Stats - moderate health
            maxHealth = 50f;
            healthRegenRate = 0f;
            healthRegenDelay = 5f;
            
            // Defense Stats - standard resistance
            damageResistance = 1f;
            knockbackResistance = 1f;
            stunResistance = 1f;
            
            // Detection Stats - balanced detection
            detectionRadius = 10f;
            detectionAngle = 120f;
            targetUpdateInterval = 0.5f;
            loseTargetTime = 5f;
            
            // AI Behavior - standard behavior
            patrolRadius = 15f;
            patrolSpeed = 3f;
            patrolWaitTime = 2f;
            chaseSpeed = 6f;
            chaseUpdateInterval = 0.2f;
            
            // Audio Settings - standard volume
            footstepVolume = 0.3f;
            attackVolume = 0.7f;
            hurtVolume = 0.8f;
            deathVolume = 0.9f;
            
            // Visual Effects - standard effects
            hitFlashDuration = 0.2f;
            deathFadeDuration = 2f;
            attackEffectScale = 1.2f;
            
            // Debug Settings
            showDebugInfo = true;
            showDetectionGizmos = false;
            showPatrolPath = false;
            showAttackRange = false;
        }
    }
    
    /// <summary>
    /// Fast Enemy Properties - quick, agile melee enemy
    /// </summary>
    [CreateAssetMenu(fileName = "FastEnemyProperties", menuName = "The Promised Run/Enemy/Fast Enemy Properties")]
    public class FastEnemyProperties : EnemyProperties {
        private void OnEnable() {
            // Combat Stats - fast attacks, lower damage
            baseDamage = 10f;
            attackRange = 1.5f;
            attackCooldown = 0.6f;
            attackWindupTime = 0.2f;
            attackRecoveryTime = 0.3f;
            
            // Movement Stats - very fast movement
            moveSpeed = 8f;
            rotationSpeed = 180f;
            acceleration = 12f;
            deceleration = 15f;
            maxSpeed = 12f;
            
            // Health Stats - lower health
            maxHealth = 30f;
            healthRegenRate = 0f;
            healthRegenDelay = 3f;
            
            // Defense Stats - low resistance
            damageResistance = 0.8f;
            knockbackResistance = 0.7f;
            stunResistance = 0.8f;
            
            // Detection Stats - wide detection
            detectionRadius = 12f;
            detectionAngle = 150f;
            targetUpdateInterval = 0.3f;
            loseTargetTime = 3f;
            
            // AI Behavior - aggressive behavior
            patrolRadius = 20f;
            patrolSpeed = 5f;
            patrolWaitTime = 1f;
            chaseSpeed = 10f;
            chaseUpdateInterval = 0.1f;
            
            // Audio Settings - quick, light sounds
            footstepVolume = 0.2f;
            attackVolume = 0.5f;
            hurtVolume = 0.6f;
            deathVolume = 0.7f;
            
            // Visual Effects - quick effects
            hitFlashDuration = 0.1f;
            deathFadeDuration = 1.5f;
            attackEffectScale = 1.1f;
            
            // Debug Settings
            showDebugInfo = true;
            showDetectionGizmos = true;
            showPatrolPath = false;
            showAttackRange = false;
        }
    }
    
    /// <summary>
    /// Tank Enemy Properties - slow, heavy melee enemy
    /// </summary>
    [CreateAssetMenu(fileName = "TankEnemyProperties", menuName = "The Promised Run/Enemy/Tank Enemy Properties")]
    public class TankEnemyProperties : EnemyProperties {
        private void OnEnable() {
            // Combat Stats - high damage, slow attacks
            baseDamage = 25f;
            attackRange = 3f;
            attackCooldown = 2f;
            attackWindupTime = 0.8f;
            attackRecoveryTime = 1f;
            
            // Movement Stats - slow but powerful
            moveSpeed = 3f;
            rotationSpeed = 90f;
            acceleration = 6f;
            deceleration = 8f;
            maxSpeed = 5f;
            
            // Health Stats - high health
            maxHealth = 100f;
            healthRegenRate = 1f;
            healthRegenDelay = 8f;
            
            // Defense Stats - high resistance
            damageResistance = 1.5f;
            knockbackResistance = 2f;
            stunResistance = 1.8f;
            
            // Detection Stats - limited but persistent
            detectionRadius = 8f;
            detectionAngle = 90f;
            targetUpdateInterval = 0.8f;
            loseTargetTime = 10f;
            
            // AI Behavior - persistent behavior
            patrolRadius = 10f;
            patrolSpeed = 2f;
            patrolWaitTime = 4f;
            chaseSpeed = 4f;
            chaseUpdateInterval = 0.4f;
            
            // Audio Settings - heavy, impactful sounds
            footstepVolume = 0.5f;
            attackVolume = 0.9f;
            hurtVolume = 0.8f;
            deathVolume = 1f;
            
            // Visual Effects - heavy effects
            hitFlashDuration = 0.3f;
            deathFadeDuration = 3f;
            attackEffectScale = 1.5f;
            
            // Debug Settings
            showDebugInfo = true;
            showDetectionGizmos = false;
            showPatrolPath = true;
            showAttackRange = true;
        }
    }
    
    /// <summary>
    /// Ranged Enemy Properties - attacks from distance
    /// </summary>
    [CreateAssetMenu(fileName = "RangedEnemyProperties", menuName = "The Promised Run/Enemy/Ranged Enemy Properties")]
    public class RangedEnemyProperties : EnemyProperties {
        private void OnEnable() {
            // Combat Stats - ranged attacks
            baseDamage = 20f;
            attackRange = 15f;
            attackCooldown = 2.5f;
            attackWindupTime = 1f;
            attackRecoveryTime = 1.5f;
            
            // Movement Stats - keeps distance
            moveSpeed = 4f;
            rotationSpeed = 100f;
            acceleration = 6f;
            deceleration = 8f;
            maxSpeed = 6f;
            
            // Health Stats - moderate health
            maxHealth = 40f;
            healthRegenRate = 0f;
            healthRegenDelay = 4f;
            
            // Defense Stats - low physical resistance
            damageResistance = 0.9f;
            knockbackResistance = 0.8f;
            stunResistance = 1.2f;
            
            // Detection Stats - long range detection
            detectionRadius = 20f;
            detectionAngle = 60f;
            targetUpdateInterval = 0.6f;
            loseTargetTime = 8f;
            
            // AI Behavior - keeps distance
            patrolRadius = 25f;
            patrolSpeed = 3f;
            patrolWaitTime = 3f;
            chaseSpeed = 4f;
            chaseUpdateInterval = 0.3f;
            
            // Audio Settings - ranged attack sounds
            footstepVolume = 0.2f;
            attackVolume = 0.6f;
            hurtVolume = 0.7f;
            deathVolume = 0.8f;
            
            // Visual Effects - projectile effects
            hitFlashDuration = 0.15f;
            deathFadeDuration = 2f;
            attackEffectScale = 1.3f;
            
            // Debug Settings
            showDebugInfo = true;
            showDetectionGizmos = true;
            showPatrolPath = false;
            showAttackRange = true;
        }
    }
    
    /// <summary>
    /// Stealth Enemy Properties - ambush predator
    /// </summary>
    [CreateAssetMenu(fileName = "StealthEnemyProperties", menuName = "The Promised Run/Enemy/Stealth Enemy Properties")]
    public class StealthEnemyProperties : EnemyProperties {
        private void OnEnable() {
            // Combat Stats - high damage, slow attacks
            baseDamage = 30f;
            attackRange = 2.5f;
            attackCooldown = 3f;
            attackWindupTime = 0.5f;
            attackRecoveryTime = 1.2f;
            
            // Movement Stats - stealthy movement
            moveSpeed = 6f;
            rotationSpeed = 150f;
            acceleration = 10f;
            deceleration = 12f;
            maxSpeed = 9f;
            
            // Health Stats - moderate health
            maxHealth = 35f;
            healthRegenRate = 2f;
            healthRegenDelay = 6f;
            
            // Defense Stats - evasion focused
            damageResistance = 0.7f;
            knockbackResistance = 0.6f;
            stunResistance = 1.5f;
            
            // Detection Stats - limited detection, high awareness
            detectionRadius = 6f;
            detectionAngle = 180f;
            targetUpdateInterval = 0.2f;
            loseTargetTime = 4f;
            
            // AI Behavior - ambush behavior
            patrolRadius = 12f;
            patrolSpeed = 2f;
            patrolWaitTime = 5f;
            chaseSpeed = 8f;
            chaseUpdateInterval = 0.15f;
            
            // Audio Settings - quiet sounds
            footstepVolume = 0.1f;
            attackVolume = 0.4f;
            hurtVolume = 0.5f;
            deathVolume = 0.3f;
            
            // Visual Effects - subtle effects
            hitFlashDuration = 0.1f;
            deathFadeDuration = 1f;
            attackEffectScale = 1.1f;
            
            // Debug Settings
            showDebugInfo = true;
            showDetectionGizmos = true;
            showPatrolPath = true;
            showAttackRange = true;
        }
    }
    
    /// <summary>
    /// Swarm Enemy Properties - weak but numerous
    /// </summary>
    [CreateAssetMenu(fileName = "SwarmEnemyProperties", menuName = "The Promised Run/Enemy/Swarm Enemy Properties")]
    public class SwarmEnemyProperties : EnemyProperties {
        private void OnEnable() {
            // Combat Stats - weak but fast attacks
            baseDamage = 5f;
            attackRange = 1f;
            attackCooldown = 0.4f;
            attackWindupTime = 0.1f;
            attackRecoveryTime = 0.2f;
            
            // Movement Stats - very fast movement
            moveSpeed = 10f;
            rotationSpeed = 200f;
            acceleration = 15f;
            deceleration = 18f;
            maxSpeed = 15f;
            
            // Health Stats - very low health
            maxHealth = 15f;
            healthRegenRate = 0f;
            healthRegenDelay = 2f;
            
            // Defense Stats - very low resistance
            damageResistance = 0.5f;
            knockbackResistance = 0.3f;
            stunResistance = 0.5f;
            
            // Detection Stats - wide but shallow detection
            detectionRadius = 8f;
            detectionAngle = 360f;
            targetUpdateInterval = 0.4f;
            loseTargetTime = 2f;
            
            // AI Behavior - swarm behavior
            patrolRadius = 5f;
            patrolSpeed = 4f;
            patrolWaitTime = 0.5f;
            chaseSpeed = 12f;
            chaseUpdateInterval = 0.25f;
            
            // Audio Settings - small, quick sounds
            footstepVolume = 0.15f;
            attackVolume = 0.3f;
            hurtVolume = 0.4f;
            deathVolume = 0.2f;
            
            // Visual Effects - quick effects
            hitFlashDuration = 0.08f;
            deathFadeDuration = 0.8f;
            attackEffectScale = 1f;
            
            // Debug Settings
            showDebugInfo = false;
            showDetectionGizmos = false;
            showPatrolPath = false;
            showAttackRange = false;
        }
    }
}
