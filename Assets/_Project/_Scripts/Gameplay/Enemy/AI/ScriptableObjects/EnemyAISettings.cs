using UnityEngine;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.ScriptableObjects {
    /// <summary>
    /// ScriptableObject containing Enemy AI configuration settings
    /// Designed for use with Odin Inspector for better editor experience
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyAISettings", menuName = "The Promised Run/Enemy/AI Settings")]
    public class EnemyAISettings : ScriptableObject {
        [Header("Movement Settings")]
        [Min(0.1f)]
        public float moveSpeed = 3f;
        
        [Min(0.1f)]
        public float rotationSpeed = 120f;
        
        [Min(0.1f)]
        public float acceleration = 8f;
        
        [Min(0.1f)]
        public float deceleration = 10f;
        
        [Header("Detection Settings")]
        [Min(1f)]
        public float detectionRadius = 8f;
        
        [Min(0.1f)]
        public float detectionAngle = 120f;
        
        [Min(0.1f)]
        public float targetUpdateInterval = 0.5f;
        
        [Header("Combat Settings")]
        [Min(0.1f)]
        public float attackRange = 2f;
        
        [Min(0.1f)]
        public float baseDamage = 10f;
        
        [Min(0.1f)]
        public float attackCooldown = 1.5f;
        
        [Min(0.1f)]
        public float attackWindupTime = 0.3f;
        
        [Header("Patrol Settings")]
        [Min(1f)]
        public float patrolRadius = 10f;
        
        [Min(0.1f)]
        public float patrolSpeed = 2f;
        
        [Min(0.1f)]
        public float patrolWaitTime = 2f;
        
        [Header("Chase Settings")]
        [Min(0.1f)]
        public float chaseSpeed = 4f;
        
        [Min(0.1f)]
        public float chaseUpdateInterval = 0.2f;
        
        [Min(0.1f)]
        public float loseTargetTime = 5f;
        
        [Header("NavMesh Settings")]
        public bool useNavMesh = true;
        
        [Min(0.1f)]
        public float navMeshStoppingDistance = 0.5f;
        
        [Min(0.1f)]
        public float navMeshUpdateInterval = 0.1f;
        
        [Header("Debug Settings")]
        public bool showDebugInfo = true;
        
        public bool showDetectionGizmos = true;
        
        public bool showPatrolPath = true;
        
        /// <summary>
        /// Validate settings and ensure they're within reasonable ranges
        /// </summary>
        public void ValidateSettings() {
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            rotationSpeed = Mathf.Max(0.1f, rotationSpeed);
            detectionRadius = Mathf.Max(1f, detectionRadius);
            attackRange = Mathf.Max(0.1f, attackRange);
            baseDamage = Mathf.Max(0.1f, baseDamage);
            attackCooldown = Mathf.Max(0.1f, attackCooldown);
        }
        
        private void OnValidate() {
            ValidateSettings();
        }
    }
}
