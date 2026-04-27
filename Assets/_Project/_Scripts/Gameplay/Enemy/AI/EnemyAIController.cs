using UnityEngine;
using System.Collections.Generic;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay.Enemy.AI.States;

namespace ThePromisedRun.Gameplay.Enemy.AI {
    /// <summary>
    /// Enemy AI Controller - manages state transitions only
    /// SOLID: Single Responsibility - Only handles AI state management
    /// SOLID: Open/Closed - Easy to add new states
    /// SOLID: Dependency Inversion - Depends on IEnemyEntity interface
    /// </summary>
    public class EnemyAIController : MonoBehaviour, IEnemyAI {
        [Header("AI Configuration")]
        [SerializeField] private LayerMask targetLayers = -1;
        [SerializeField] private bool debugMode = false;
        
        // IEnemyAI Implementation
        public EnemyAIState CurrentState { get; private set; }
        public bool IsActive { get; set; } = true;
        
        // Events
        public System.Action<EnemyAIState> OnStateChanged { get; set; }
        public System.Action<IDamageable> OnTargetAcquired { get; set; }
        public System.Action OnTargetLost { get; set; }
        
        // Private state
        private IEnemyEntity _enemy;
        private Dictionary<EnemyAIState, EnemyStateBase> _states;
        private EnemyStateBase _currentStateInstance;
        
        // Properties
        public IEnemyEntity Enemy => _enemy;
        
        private void Awake() {
            InitializeStates();
            InitializeEvents();
        }
        
        private void Start() {
            // Find enemy entity if not assigned
            if (_enemy == null) {
                _enemy = GetComponent<Enemy>();
                if (_enemy == null) {
                    Debug.LogError("[EnemyAIController] No Enemy component found!");
                    return;
                }
            }
            
            Initialize(_enemy);
        }
        
        private void Update() {
            if (!IsActive || _currentStateInstance == null) return;
            
            _currentStateInstance.Update();
        }
        
        #region IEnemyAI Implementation
        public void Initialize(IEnemyEntity enemy) {
            _enemy = enemy;
            ChangeState(EnemyAIState.Idle);
            
            if (debugMode) {
                Debug.Log($"[EnemyAIController] Initialized for {_enemy.GameObject.name}");
            }
        }
        
        public void UpdateAI() {
            // This method is called by IEnemyAI interface, but we handle updates in Update()
            // Keeping this for interface compatibility
        }
        
        public void ChangeState(EnemyAIState newState) {
            if (CurrentState == newState) return;
            
            // Exit current state
            if (_currentStateInstance != null) {
                _currentStateInstance.Exit();
            }
            
            // Enter new state
            var oldState = CurrentState;
            CurrentState = newState;
            _currentStateInstance = _states[newState];
            _currentStateInstance.Enter();
            
            // Fire event
            OnStateChanged?.Invoke(newState);
            
            if (debugMode) {
                Debug.Log($"[EnemyAIController] State changed: {oldState} -> {newState}");
            }
        }
        
        public void SetTarget(IDamageable target) {
            if (_enemy != null) {
                _enemy.SetTarget(target);
                OnTargetAcquired?.Invoke(target);
            }
        }
        
        public void ClearTarget() {
            if (_enemy != null) {
                _enemy.ClearTarget();
                OnTargetLost?.Invoke();
            }
        }
        #endregion
        
        #region Private Methods
        private void InitializeStates() {
            _states = new Dictionary<EnemyAIState, EnemyStateBase>();
            
            // States will be created when enemy is available
            // This allows for proper dependency injection
        }
        
        private void InitializeEvents() {
            OnStateChanged = (state) => { };
            OnTargetAcquired = (target) => { };
            OnTargetLost = () => { };
        }
        
        private void CreateStates() {
            if (_enemy == null) return;
            
            // Create state instances
            _states[EnemyAIState.Idle] = new IdleState(_enemy, this);
            _states[EnemyAIState.Chase] = new ChaseState(_enemy, this);
            _states[EnemyAIState.Attack] = new AttackState(_enemy, this);
            _states[EnemyAIState.Dead] = new DeadState(_enemy, this);
            _states[EnemyAIState.Stunned] = new StunnedState(_enemy, this);
            _states[EnemyAIState.Patrol] = new PatrolState(_enemy, this);
            
            if (debugMode) {
                Debug.Log($"[EnemyAIController] Created {_states.Count} states");
            }
        }
        #endregion
        
        #region Public Helper Methods
        /// <summary>
        /// Force state change (for testing or external control)
        /// </summary>
        public void ForceState(EnemyAIState state) {
            ChangeState(state);
        }
        
        /// <summary>
        /// Get current state name for debugging
        /// </summary>
        public string GetCurrentStateName() {
            return CurrentState.ToString();
        }
        
        /// <summary>
        /// Handle damage events and pass to current state
        /// </summary>
        public void OnDamaged(DamageInfo damageInfo) {
            if (_currentStateInstance != null) {
                _currentStateInstance.OnDamaged(damageInfo);
                
                // Check for stun from overload damage
                if (damageInfo.IsOverloadBoosted && CurrentState != EnemyAIState.Stunned && CurrentState != EnemyAIState.Dead) {
                    ChangeState(EnemyAIState.Stunned);
                }
            }
        }
        #endregion
        
        #region Unity Events
        private void OnEnable() {
            // Re-initialize when enabled
            if (_enemy != null && _states.Count == 0) {
                CreateStates();
                ChangeState(EnemyAIState.Idle);
            }
        }
        
        private void OnDisable() {
            IsActive = false;
        }
        #endregion
    }
}
