using UnityEngine;
using UnityEngine.Events;

namespace ThePromisedRun.Core.Interfaces {
    /// <summary>
    /// Main interface for player controller
    /// SOLID: Interface Segregation - Single Responsibility for player control
    /// </summary>
    public interface IPlayerController {
        /// <summary>
        /// Player's Rigidbody for physics
        /// </summary>
        Rigidbody Rb { get; }
        
        /// <summary>
        /// Player's Animator
        /// </summary>
        Animator Anim { get; }
        
        /// <summary>
        /// Input reader component - using InputSystem
        /// </summary>
        Vector2 MoveInput { get; }
        
        /// <summary>
        /// Visual transform for rotation
        /// </summary>
        Transform Visual { get; }
        
        /// <summary>
        /// Whether player is grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Whether player is alive
        /// </summary>
        bool IsAlive { get; }
        
        /// <summary>
        /// Current chaos meter value
        /// </summary>
        float ChaosMeter { get; }
        
        /// <summary>
        /// Whether overload is active
        /// </summary>
        bool IsOverloaded { get; }
        
        /// <summary>
        /// Current overload timer
        /// </summary>
        float OverloadTimer { get; }
        
        /// <summary>
        /// Current cooldown timer
        /// </summary>
        float CooldownTimer { get; }
        
        /// <summary>
        /// Event fired when chaos changes (normalized 0-1)
        /// </summary>
        UnityEvent<float> OnChaosChanged { get; }
        
        /// <summary>
        /// Event fired when overload starts
        /// </summary>
        UnityEvent OnOverloadStarted { get; }
        
        /// <summary>
        /// Event fired when overload ends
        /// </summary>
        UnityEvent OnOverloadEnded { get; }
        
        /// <summary>
        /// Add chaos to the meter
        /// </summary>
        void AddChaos(float amount, ChaosSource source = ChaosSource.Manual);
        
        /// <summary>
        /// Initiate overload state
        /// </summary>
        void InitiateOverload();
        
        /// <summary>
        /// End overload state
        /// </summary>
        void EndOverload();
    }
    
    /// <summary>
    /// Source of chaos for tracking
    /// </summary>
    public enum ChaosSource {
        Manual,
        Attack,
        Jump,
        Damage,
        Overload
    }
}