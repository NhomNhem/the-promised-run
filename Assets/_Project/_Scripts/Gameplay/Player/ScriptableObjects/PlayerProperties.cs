using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ThePromisedRun.Gameplay.Player.ScriptableObjects {
    /// <summary>
    /// ScriptableObject containing all player properties and stats.
    /// Assign to PlayerController to configure gameplay balance without code changes.
    /// </summary>
    #if ODIN_INSPECTOR
    [InfoBox("ScriptableObject containing all player properties. Assign to PlayerController to configure gameplay balance without code changes.")]
    #endif
    [CreateAssetMenu(fileName = "PlayerProperties", menuName = "The Promised Run/Player/Player Properties")]
    public class PlayerProperties : ScriptableObject {

        #region Movement

        #if ODIN_INSPECTOR
        [BoxGroup("Movement")]
        #else
        [Header("Movement Settings")]
        #endif
        [Min(0.1f)]
        public float moveSpeed = 8f;

        #if ODIN_INSPECTOR
        [BoxGroup("Movement")]
        #endif
        [Min(0.1f)]
        public float jumpForce = 12f;

        #if ODIN_INSPECTOR
        [BoxGroup("Movement")]
        #endif
        [Min(0.1f)]
        public float rotationSpeed = 10f;

        #if ODIN_INSPECTOR
        [BoxGroup("Movement")]
        #endif
        [Min(0.1f)]
        [Range(1f, 5f)]
        public float fallGravityMultiplier = 2.5f;

        #if ODIN_INSPECTOR
        [BoxGroup("Movement")]
        #endif
        [Range(0f, 1f)]
        public float airControl = 0.5f;

        #endregion

        #region Health

        #if ODIN_INSPECTOR
        [BoxGroup("Health")]
        #else
        [Header("Health Settings")]
        #endif
        [Min(1f)]
        public float maxHealth = 100f;

        #if ODIN_INSPECTOR
        [BoxGroup("Health")]
        #endif
        [Min(0f)]
        public float healthRegenRate = 0f;

        #if ODIN_INSPECTOR
        [BoxGroup("Health")]
        #endif
        [Min(0f)]
        public float healthRegenDelay = 3f;

        #endregion

        #region Combat

        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #else
        [Header("Combat Settings")]
        #endif
        [Min(0.1f)]
        public float attackCooldown = 0.15f;

        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Min(0.1f)]
        [Range(0.1f, 2f)]
        public float comboWindow = 0.6f;

        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Min(1)]
        [Range(1, 5)]
        public int maxComboCount = 3;

        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Min(0.1f)]
        [Range(0.5f, 3f)]
        public float damageMultiplier = 1f;

        // Attack step-forward impulse per combo hit (index 0=hit1, 1=hit2, 2=hit3)
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        public float[] attackStepForce = { 2.5f, 3.5f, 5f };

        // Per-hit cooldown inside combo (min time between ExecuteHit calls)
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Min(0f)]
        public float comboHitCooldown = 0.12f;

        // Time after last hit before returning to Locomotion
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Min(0.1f)]
        public float attackExitDelay = 0.5f;

        // Hitstop duration and timescale on confirmed hit
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Min(0f)]
        public float hitStopDuration = 0.06f;

        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Range(0.01f, 0.5f)]
        public float hitStopTimeScale = 0.05f;

        // Combo clip durations (seconds) — must match actual animation clip lengths
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        public float[] comboClipDurations = { 1.17f, 0.93f, 1.37f };

        // Fraction of clip that must play before chaining to next hit (0=instant, 1=full clip)
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Range(0.1f, 0.9f)]
        public float comboChainFraction = 0.5f;

        // Time after last hit before returning to Locomotion
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Min(0f)]
        public float comboFinishDelay = 0.15f;

        // CrossFade blend time between attack clips
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Range(0f, 0.2f)]
        public float attackBlendTime = 0.05f;

        // Movement speed multiplier while attacking (0=frozen, 1=full speed)
        #if ODIN_INSPECTOR
        [BoxGroup("Combat")]
        #endif
        [Range(0f, 1f)]
        public float attackMoveDamping = 0.3f;

        #endregion

        #region Chaos System

        #if ODIN_INSPECTOR
        [FoldoutGroup("Chaos System")]
        #else
        [Header("System Overload Settings")]
        #endif
        [Min(0.1f)]
        public float overloadDuration = 3f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Chaos System")]
        #endif
        [Min(0.1f)]
        public float overloadCooldown = 5f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Chaos System")]
        #endif
        [Min(0.1f)]
        public float maxChaosThreshold = 100f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Chaos System")]
        #endif
        [Min(0.1f)]
        public float chaosDecayRate = 10f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Chaos System")]
        #endif
        [Min(0f)]
        public float chaosPerHit = 15f;

        #endregion

        #region Detection

        #if ODIN_INSPECTOR
        [FoldoutGroup("Detection")]
        #else
        [Header("Detection Settings")]
        #endif
        [Min(1f)]
        public float enemyDetectionRadius = 10f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Detection")]
        #endif
        [Range(0f, 360f)]
        public float enemyDetectionAngle = 120f;

        #endregion

        #region Juice & Audio

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #else
        [Header("Juice Settings")]
        #endif
        [Min(0f)]
        public float landingImpactForce = 5f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #endif
        [Min(0.1f)]
        [Range(0.1f, 1f)]
        public float jumpJuiceDuration = 0.2f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #endif
        [Min(0.1f)]
        [Range(0.1f, 1f)]
        public float attackJuiceDuration = 0.15f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #endif
        [Min(0.1f)]
        [Range(0.1f, 1f)]
        public float hurtJuiceDuration = 0.3f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #else
        [Header("Audio Settings")]
        #endif
        [Range(0f, 1f)]
        public float footstepVolume = 0.5f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #endif
        [Range(0f, 1f)]
        public float jumpVolume = 0.7f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #endif
        [Range(0f, 1f)]
        public float attackVolume = 0.8f;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Juice & Audio")]
        #endif
        [Range(0f, 1f)]
        public float hurtVolume = 0.9f;

        #endregion

        #region Debug

        #if ODIN_INSPECTOR
        [FoldoutGroup("Debug")]
        #else
        [Header("Debug Settings")]
        #endif
        public bool showDebugInfo = true;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Debug")]
        #endif
        public bool showMovementGizmos = false;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Debug")]
        #endif
        public bool showDetectionGizmos = false;

        #endregion

        #region Methods

        /// <summary>
        /// Validate all settings and ensure they're within reasonable ranges
        /// </summary>
        public void ValidateSettings() {
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            jumpForce = Mathf.Max(0.1f, jumpForce);
            rotationSpeed = Mathf.Max(0.1f, rotationSpeed);
            fallGravityMultiplier = Mathf.Max(0.1f, fallGravityMultiplier);
            airControl = Mathf.Clamp01(airControl);

            maxHealth = Mathf.Max(1f, maxHealth);
            healthRegenRate = Mathf.Max(0f, healthRegenRate);
            healthRegenDelay = Mathf.Max(0f, healthRegenDelay);

            attackCooldown = Mathf.Max(0.1f, attackCooldown);
            comboWindow = Mathf.Max(0.1f, comboWindow);
            maxComboCount = Mathf.Max(1, maxComboCount);
            damageMultiplier = Mathf.Max(0.1f, damageMultiplier);

            overloadDuration = Mathf.Max(0.1f, overloadDuration);
            overloadCooldown = Mathf.Max(0.1f, overloadCooldown);
            maxChaosThreshold = Mathf.Max(0.1f, maxChaosThreshold);
            chaosDecayRate = Mathf.Max(0f, chaosDecayRate);
            chaosPerHit = Mathf.Max(0f, chaosPerHit);

            enemyDetectionRadius = Mathf.Max(1f, enemyDetectionRadius);
            enemyDetectionAngle = Mathf.Clamp(enemyDetectionAngle, 0f, 360f);

            landingImpactForce = Mathf.Max(0f, landingImpactForce);
            jumpJuiceDuration = Mathf.Max(0.1f, jumpJuiceDuration);
            attackJuiceDuration = Mathf.Max(0.1f, attackJuiceDuration);
            hurtJuiceDuration = Mathf.Max(0.1f, hurtJuiceDuration);

            footstepVolume = Mathf.Clamp01(footstepVolume);
            jumpVolume = Mathf.Clamp01(jumpVolume);
            attackVolume = Mathf.Clamp01(attackVolume);
            hurtVolume = Mathf.Clamp01(hurtVolume);
        }

        private void OnValidate() {
            ValidateSettings();
        }

        /// <summary>
        /// Create a copy of these properties
        /// </summary>
        public PlayerProperties Clone() {
            PlayerProperties clone = CreateInstance<PlayerProperties>();
            clone.moveSpeed = moveSpeed;
            clone.jumpForce = jumpForce;
            clone.rotationSpeed = rotationSpeed;
            clone.fallGravityMultiplier = fallGravityMultiplier;
            clone.airControl = airControl;
            clone.maxHealth = maxHealth;
            clone.healthRegenRate = healthRegenRate;
            clone.healthRegenDelay = healthRegenDelay;
            clone.attackCooldown = attackCooldown;
            clone.comboWindow = comboWindow;
            clone.maxComboCount = maxComboCount;
            clone.damageMultiplier = damageMultiplier;
            clone.overloadDuration = overloadDuration;
            clone.overloadCooldown = overloadCooldown;
            clone.maxChaosThreshold = maxChaosThreshold;
            clone.chaosDecayRate = chaosDecayRate;
            clone.chaosPerHit = chaosPerHit;
            clone.enemyDetectionRadius = enemyDetectionRadius;
            clone.enemyDetectionAngle = enemyDetectionAngle;
            clone.landingImpactForce = landingImpactForce;
            clone.jumpJuiceDuration = jumpJuiceDuration;
            clone.attackJuiceDuration = attackJuiceDuration;
            clone.hurtJuiceDuration = hurtJuiceDuration;
            clone.footstepVolume = footstepVolume;
            clone.jumpVolume = jumpVolume;
            clone.attackVolume = attackVolume;
            clone.hurtVolume = hurtVolume;
            clone.showDebugInfo = showDebugInfo;
            clone.showMovementGizmos = showMovementGizmos;
            clone.showDetectionGizmos = showDetectionGizmos;
            return clone;
        }

        #endregion
    }
}
