using UnityEngine;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.ScriptableObjects {
    /// <summary>
    /// Base class for AI conditions used in decision making
    /// Designed for use with Odin Inspector for visual condition editing
    /// </summary>
    [System.Serializable]
    public abstract class AICondition {
        public string conditionName;
        [TextArea(2, 3)]
        public string description;
        
        public abstract bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController);
    }
    
    /// <summary>
    /// Condition that checks if enemy has a target
    /// </summary>
    [System.Serializable]
    public class HasTargetCondition : AICondition {
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            return enemy != null && enemy.HasTarget;
        }
    }
    
    /// <summary>
    /// Condition that checks distance to target
    /// </summary>
    [System.Serializable]
    public class DistanceCondition : AICondition {
        public ComparisonType comparison = ComparisonType.LessThan;
        public float distance = 5f;
        
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            if (enemy == null || !enemy.HasTarget) return false;
            
            var targetPos = enemy.LastKnownTargetPosition;
            var currentPos = enemy.GameObject.transform.position;
            var actualDistance = Vector3.Distance(currentPos, targetPos);
            
            switch (comparison) {
                case ComparisonType.LessThan:
                    return actualDistance < distance;
                case ComparisonType.LessThanOrEqual:
                    return actualDistance <= distance;
                case ComparisonType.GreaterThan:
                    return actualDistance > distance;
                case ComparisonType.GreaterThanOrEqual:
                    return actualDistance >= distance;
                case ComparisonType.Equal:
                    return Mathf.Approximately(actualDistance, distance);
                default:
                    return false;
            }
        }
    }
    
    /// <summary>
    /// Condition that checks enemy health percentage
    /// </summary>
    [System.Serializable]
    public class HealthCondition : AICondition {
        public ComparisonType comparison = ComparisonType.LessThan;
        [Range(0, 1)]
        public float healthPercentage = 0.5f;
        
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            if (enemy == null) return false;
            
            var healthRatio = enemy.Health / enemy.MaxHealth;
            
            switch (comparison) {
                case ComparisonType.LessThan:
                    return healthRatio < healthPercentage;
                case ComparisonType.LessThanOrEqual:
                    return healthRatio <= healthPercentage;
                case ComparisonType.GreaterThan:
                    return healthRatio > healthPercentage;
                case ComparisonType.GreaterThanOrEqual:
                    return healthRatio >= healthPercentage;
                case ComparisonType.Equal:
                    return Mathf.Approximately(healthRatio, healthPercentage);
                default:
                    return false;
            }
        }
    }
    
    /// <summary>
    /// Condition that checks if target is in attack range
    /// </summary>
    [System.Serializable]
    public class AttackRangeCondition : AICondition {
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            if (enemy == null || !enemy.HasTarget) return false;
            
            return enemy.IsTargetInRange(enemy.CurrentTarget);
        }
    }
    
    /// <summary>
    /// Condition that checks if enemy can attack
    /// </summary>
    [System.Serializable]
    public class CanAttackCondition : AICondition {
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            if (enemy == null) return false;
            
            return enemy.CanAttack;
        }
    }
    
    /// <summary>
    /// Condition that checks time since last seen target
    /// </summary>
    [System.Serializable]
    public class TimeSinceLastSeenCondition : AICondition {
        public ComparisonType comparison = ComparisonType.GreaterThan;
        public float time = 3f;
        
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            if (enemy == null) return false;
            
            switch (comparison) {
                case ComparisonType.LessThan:
                    return enemy.TimeSinceLastSeenTarget < time;
                case ComparisonType.LessThanOrEqual:
                    return enemy.TimeSinceLastSeenTarget <= time;
                case ComparisonType.GreaterThan:
                    return enemy.TimeSinceLastSeenTarget > time;
                case ComparisonType.GreaterThanOrEqual:
                    return enemy.TimeSinceLastSeenTarget >= time;
                case ComparisonType.Equal:
                    return Mathf.Approximately(enemy.TimeSinceLastSeenTarget, time);
                default:
                    return false;
            }
        }
    }
    
    /// <summary>
    /// Condition that combines multiple conditions with AND/OR logic
    /// </summary>
    [System.Serializable]
    public class CompositeCondition : AICondition {
        public LogicalOperator logicalOperator = LogicalOperator.And;
        public AICondition[] conditions;
        
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            if (conditions == null || conditions.Length == 0) return true;
            
            switch (logicalOperator) {
                case LogicalOperator.And:
                    foreach (var condition in conditions) {
                        if (condition == null || !condition.Evaluate(enemy, aiController))
                            return false;
                    }
                    return true;
                    
                case LogicalOperator.Or:
                    foreach (var condition in conditions) {
                        if (condition != null && condition.Evaluate(enemy, aiController))
                            return true;
                    }
                    return false;
                    
                default:
                    return false;
            }
        }
    }
    
    /// <summary>
    /// Condition that checks if enemy is in a specific state
    /// </summary>
    [System.Serializable]
    public class StateCondition : AICondition {
        public EnemyAIState requiredState;
        
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            return aiController != null && aiController.CurrentState == requiredState;
        }
    }
    
    /// <summary>
    /// Condition that checks random chance
    /// </summary>
    [System.Serializable]
    public class RandomCondition : AICondition {
        [Range(0, 1)]
        public float probability = 0.5f;
        
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            return Random.value <= probability;
        }
    }
    
    /// <summary>
    /// Condition that checks if enemy is stunned
    /// </summary>
    [System.Serializable]
    public class StunnedCondition : AICondition {
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            return aiController != null && aiController.CurrentState == EnemyAIState.Stunned;
        }
    }
    
    /// <summary>
    /// Condition that checks if enemy is dead
    /// </summary>
    [System.Serializable]
    public class DeadCondition : AICondition {
        public override bool Evaluate(IEnemyEntity enemy, IEnemyAI aiController) {
            return enemy != null && !enemy.IsAlive;
        }
    }
    
    public enum ComparisonType {
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Equal
    }
    
    public enum LogicalOperator {
        And,
        Or
    }
}
