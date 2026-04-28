using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Core.Systems;

namespace ThePromisedRun.Gameplay.Enemy.AI.ScriptableObjects {
    /// <summary>
    /// Example Enemy AI Behavior demonstrating the AI system
    /// This shows how to create complex AI behaviors using Scriptable Objects
    /// </summary>
    [CreateAssetMenu(fileName = "ExampleEnemyBehavior", menuName = "The Promised Run/Enemy/Example AI Behavior")]
    public class ExampleEnemyAIBehavior : EnemyAIBehavior {
        private void OnEnable() {
            behaviorName = "Aggressive Melee Enemy";
            description = "Enemy that aggressively pursues and attacks targets with high priority on combat";
            
            // Initialize state priorities
            statePriorities = new StatePriority[] {
                new StatePriority {
                    targetState = EnemyAIState.Dead,
                    priority = 100f,
                    condition = new DeadCondition()
                },
                new StatePriority {
                    targetState = EnemyAIState.Stunned,
                    priority = 90f,
                    condition = new StunnedCondition()
                },
                new StatePriority {
                    targetState = EnemyAIState.Attack,
                    priority = 80f,
                    condition = new CompositeCondition {
                        logicalOperator = LogicalOperator.And,
                        conditions = new AICondition[] {
                            new HasTargetCondition(),
                            new AttackRangeCondition(),
                            new CanAttackCondition()
                        }
                    }
                },
                new StatePriority {
                    targetState = EnemyAIState.Chase,
                    priority = 70f,
                    condition = new CompositeCondition {
                        logicalOperator = LogicalOperator.And,
                        conditions = new AICondition[] {
                            new HasTargetCondition(),
                            new DistanceCondition {
                                comparison = ComparisonType.GreaterThan,
                                distance = 2f
                            }
                        }
                    }
                },
                new StatePriority {
                    targetState = EnemyAIState.Patrol,
                    priority = 30f,
                    condition = new CompositeCondition {
                        logicalOperator = LogicalOperator.And,
                        conditions = new AICondition[] {
                            new TimeSinceLastSeenCondition {
                                comparison = ComparisonType.GreaterThan,
                                time = 3f
                            }
                        }
                    }
                },
                new StatePriority {
                    targetState = EnemyAIState.Idle,
                    priority = 20f,
                    condition = new CompositeCondition {
                        logicalOperator = LogicalOperator.And,
                        conditions = new AICondition[] {
                            new HasTargetCondition {
                                conditionName = "No Target",
                                description = "Enemy has no target"
                            }
                        }
                    }
                }
            };
            
            // Initialize decision making
            decisions = new AIDecision[] {
                new AIDecision {
                    decisionName = "Aggressive Attack",
                    resultingState = EnemyAIState.Attack,
                    baseScore = 50f,
                    condition = new CompositeCondition {
                        logicalOperator = LogicalOperator.And,
                        conditions = new AICondition[] {
                            new HasTargetCondition(),
                            new AttackRangeCondition()
                        }
                    },
                    scoreModifiers = new ScoreModifier[] {
                        new ScoreModifier {
                            condition = new HealthCondition {
                                comparison = ComparisonType.LessThan,
                                healthPercentage = 0.5f
                            },
                            multiplier = 1.5f,
                            bonus = 20f
                        }
                    }
                },
                new AIDecision {
                    decisionName = "Strategic Chase",
                    resultingState = EnemyAIState.Chase,
                    baseScore = 30f,
                    condition = new HasTargetCondition(),
                    scoreModifiers = new ScoreModifier[] {
                        new ScoreModifier {
                            condition = new DistanceCondition {
                                comparison = ComparisonType.LessThan,
                                distance = 5f
                            },
                            multiplier = 2f,
                            bonus = 10f
                        }
                    }
                }
            };
            
            // Initialize response rules
            responseRules = new AIResponseRule[] {
                new AIResponseRule {
                    ruleName = "Stun Response",
                    triggerDamageType = DamageType.Overload,
                    responseState = EnemyAIState.Stunned,
                    responseChance = 1f,
                    condition = null // Always respond to overload damage
                },
                new AIResponseRule {
                    ruleName = "Fire Response",
                    triggerDamageType = DamageType.Fire,
                    responseState = EnemyAIState.Chase,
                    responseChance = 0.8f,
                    condition = new HealthCondition {
                        comparison = ComparisonType.GreaterThan,
                        healthPercentage = 0.3f
                    }
                },
                new AIResponseRule {
                    ruleName = "Critical Response",
                    triggerDamageType = DamageType.Physical,
                    responseState = EnemyAIState.Stunned,
                    responseChance = 0.3f,
                    condition = new HealthCondition {
                        comparison = ComparisonType.LessThan,
                        healthPercentage = 0.2f
                    }
                }
            };
            
            // Initialize behavior modifiers
            modifiers = new BehaviorModifier[] {
                new BehaviorModifier {
                    modifierName = "Berserk Mode",
                    type = ModificationType.SpeedMultiplier,
                    value = 1.5f,
                    condition = new HealthCondition {
                        comparison = ComparisonType.LessThan,
                        healthPercentage = 0.3f
                    }
                },
                new BehaviorModifier {
                    modifierName = "Desperate Damage",
                    type = ModificationType.DamageMultiplier,
                    value = 1.2f,
                    condition = new HealthCondition {
                        comparison = ComparisonType.LessThan,
                        healthPercentage = 0.25f
                    }
                },
                new BehaviorModifier {
                    modifierName = "Enhanced Detection",
                    type = ModificationType.DetectionRadius,
                    value = 1.3f,
                    condition = new TimeSinceLastSeenCondition {
                        comparison = ComparisonType.GreaterThan,
                        time = 5f
                    }
                }
            };
        }
    }
    
    /// <summary>
    /// Example Enemy AI Settings for different enemy types
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeEnemySettings", menuName = "The Promised Run/Enemy/Melee Enemy Settings")]
    public class MeleeEnemySettings : EnemyAISettings {
        private void OnEnable() {
            // Movement settings for melee enemies
            moveSpeed = 4f;
            rotationSpeed = 180f;
            acceleration = 12f;
            deceleration = 15f;
            
            // Detection settings
            detectionRadius = 8f;
            detectionAngle = 120f;
            targetUpdateInterval = 0.3f;
            
            // Combat settings
            attackRange = 2f;
            baseDamage = 15f;
            attackCooldown = 1.2f;
            attackWindupTime = 0.4f;
            
            // Patrol settings
            patrolRadius = 12f;
            patrolSpeed = 2.5f;
            patrolWaitTime = 3f;
            
            // Chase settings
            chaseSpeed = 5f;
            chaseUpdateInterval = 0.15f;
            loseTargetTime = 4f;
            
            // NavMesh settings
            useNavMesh = true;
            navMeshStoppingDistance = 0.3f;
            navMeshUpdateInterval = 0.1f;
            
            // Debug settings
            showDebugInfo = true;
            showDetectionGizmos = true;
            showPatrolPath = true;
        }
    }
    
    /// <summary>
    /// Example Enemy AI Settings for ranged enemies
    /// </summary>
    [CreateAssetMenu(fileName = "RangedEnemySettings", menuName = "The Promised Run/Enemy/Ranged Enemy Settings")]
    public class RangedEnemySettings : EnemyAISettings {
        private void OnEnable() {
            // Movement settings for ranged enemies
            moveSpeed = 3f;
            rotationSpeed = 120f;
            acceleration = 8f;
            deceleration = 10f;
            
            // Detection settings (longer range for ranged)
            detectionRadius = 15f;
            detectionAngle = 90f;
            targetUpdateInterval = 0.5f;
            
            // Combat settings
            attackRange = 12f;
            baseDamage = 20f;
            attackCooldown = 2f;
            attackWindupTime = 0.6f;
            
            // Patrol settings
            patrolRadius = 20f;
            patrolSpeed = 2f;
            patrolWaitTime = 4f;
            
            // Chase settings (keep distance)
            chaseSpeed = 3.5f;
            chaseUpdateInterval = 0.3f;
            loseTargetTime = 6f;
            
            // NavMesh settings
            useNavMesh = true;
            navMeshStoppingDistance = 8f; // Keep distance for ranged attacks
            navMeshUpdateInterval = 0.2f;
            
            // Debug settings
            showDebugInfo = true;
            showDetectionGizmos = true;
            showPatrolPath = true;
        }
    }
    
    /// <summary>
    /// Example Enemy AI Settings for tank enemies
    /// </summary>
    [CreateAssetMenu(fileName = "TankEnemySettings", menuName = "The Promised Run/Enemy/Tank Enemy Settings")]
    public class TankEnemySettings : EnemyAISettings {
        private void OnEnable() {
            // Movement settings for tank enemies (slower but more persistent)
            moveSpeed = 2.5f;
            rotationSpeed = 90f;
            acceleration = 6f;
            deceleration = 8f;
            
            // Detection settings
            detectionRadius = 10f;
            detectionAngle = 150f;
            targetUpdateInterval = 0.4f;
            
            // Combat settings (high damage, slow attack)
            attackRange = 3f;
            baseDamage = 30f;
            attackCooldown = 2.5f;
            attackWindupTime = 0.8f;
            
            // Patrol settings
            patrolRadius = 8f;
            patrolSpeed = 1.5f;
            patrolWaitTime = 5f;
            
            // Chase settings (very persistent)
            chaseSpeed = 3f;
            chaseUpdateInterval = 0.25f;
            loseTargetTime = 10f;
            
            // NavMesh settings
            useNavMesh = true;
            navMeshStoppingDistance = 0.2f;
            navMeshUpdateInterval = 0.15f;
            
            // Debug settings
            showDebugInfo = true;
            showDetectionGizmos = true;
            showPatrolPath = true;
        }
    }
}
