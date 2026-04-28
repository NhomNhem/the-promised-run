using UnityEngine;
using System.Collections.Generic;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Core.Systems;

namespace ThePromisedRun.Gameplay.Enemy.AI.ScriptableObjects {
    /// <summary>
    /// ScriptableObject defining Enemy AI behavior patterns and decision trees
    /// Designed for use with Odin Inspector for visual behavior editing
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyAIBehavior", menuName = "The Promised Run/Enemy/AI Behavior")]
    public class EnemyAIBehavior : ScriptableObject {
        [Header("Behavior Configuration")]
        public string behaviorName = "Default Behavior";
        [TextArea(3, 5)]
        public string description = "Default enemy AI behavior";
        
        [Header("State Priorities")]
        public StatePriority[] statePriorities;
        
        [Header("Decision Making")]
        public AIDecision[] decisions;
        
        [Header("Response Rules")]
        public AIResponseRule[] responseRules;
        
        [Header("Behavior Modifiers")]
        public BehaviorModifier[] modifiers;
        
        /// <summary>
        /// Get the highest priority state based on current conditions
        /// </summary>
        public EnemyAIState GetHighestPriorityState(IEnemyEntity enemy, IEnemyAI aiController) {
            EnemyAIState bestState = EnemyAIState.Idle;
            float highestPriority = float.MinValue;
            
            foreach (var priority in statePriorities) {
                if (priority.condition != null && priority.condition.Evaluate(enemy, aiController)) {
                    if (priority.priority > highestPriority) {
                        highestPriority = priority.priority;
                        bestState = priority.targetState;
                    }
                }
            }
            
            return bestState;
        }
        
        /// <summary>
        /// Process all decision rules and return the best action
        /// </summary>
        public AIDecision GetBestDecision(IEnemyEntity enemy, IEnemyAI aiController) {
            AIDecision bestDecision = null;
            float highestScore = float.MinValue;
            
            foreach (var decision in decisions) {
                if (decision.condition != null && decision.condition.Evaluate(enemy, aiController)) {
                    float score = decision.CalculateScore(enemy, aiController);
                    if (score > highestScore) {
                        highestScore = score;
                        bestDecision = decision;
                    }
                }
            }
            
            return bestDecision;
        }
        
        /// <summary>
        /// Apply behavior modifiers to enemy properties
        /// </summary>
        public void ApplyModifiers(IEnemyEntity enemy) {
            foreach (var modifier in modifiers) {
                if (modifier.condition != null && modifier.condition.Evaluate(enemy, null)) {
                    modifier.Apply(enemy);
                }
            }
        }
        
        private void OnValidate() {
            // Ensure arrays are not null
            if (statePriorities == null) statePriorities = new StatePriority[0];
            if (decisions == null) decisions = new AIDecision[0];
            if (responseRules == null) responseRules = new AIResponseRule[0];
            if (modifiers == null) modifiers = new BehaviorModifier[0];
        }
    }
    
    [System.Serializable]
    public class StatePriority {
        public EnemyAIState targetState;
        [Range(0, 100)]
        public float priority = 1f;
        public AICondition condition;
    }
    
    [System.Serializable]
    public class AIDecision {
        public string decisionName;
        public EnemyAIState resultingState;
        public AICondition condition;
        [Range(0, 100)]
        public float baseScore = 1f;
        public ScoreModifier[] scoreModifiers;
        
        public float CalculateScore(IEnemyEntity enemy, IEnemyAI aiController) {
            float score = baseScore;
            
            foreach (var modifier in scoreModifiers) {
                if (modifier.condition != null && modifier.condition.Evaluate(enemy, aiController)) {
                    score *= modifier.multiplier;
                    score += modifier.bonus;
                }
            }
            
            return score;
        }
    }
    
    [System.Serializable]
    public class AIResponseRule {
        public string ruleName;
        public DamageType triggerDamageType;
        public EnemyAIState responseState;
        public float responseChance = 1f;
        public AICondition condition;
        
        public bool ShouldRespond(DamageInfo damageInfo, IEnemyEntity enemy) {
            // For now, we'll use IsOverloadBoosted as a simple damage type indicator
            // In a full implementation, you might want to extend DamageInfo to include damage type
            bool isOverload = damageInfo.IsOverloadBoosted;
            
            if (triggerDamageType == DamageType.Overload && !isOverload) return false;
            if (triggerDamageType != DamageType.Overload && isOverload) return false;
            
            if (condition != null && !condition.Evaluate(enemy, null)) return false;
            
            return Random.value <= responseChance;
        }
    }
    
    [System.Serializable]
    public class BehaviorModifier {
        public string modifierName;
        public ModificationType type;
        public float value = 1f;
        public AICondition condition;
        
        public void Apply(IEnemyEntity enemy) {
            if (enemy is Enemy enemyComponent) {
                switch (type) {
                    case ModificationType.SpeedMultiplier:
                        // Apply speed modifier (would need to be implemented in Enemy class)
                        break;
                    case ModificationType.DamageMultiplier:
                        // Apply damage modifier
                        break;
                    case ModificationType.DetectionRadius:
                        // Apply detection radius modifier
                        break;
                }
            }
        }
    }
    
    [System.Serializable]
    public class ScoreModifier {
        public AICondition condition;
        public float multiplier = 1f;
        public float bonus = 0f;
    }
    
    public enum ModificationType {
        SpeedMultiplier,
        DamageMultiplier,
        DetectionRadius,
        AttackRange,
        AttackCooldown
    }
}
