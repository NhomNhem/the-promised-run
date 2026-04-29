using UnityEngine;

namespace ThePromisedRun.Core.Interfaces {
    /// <summary>
    /// Interface for entities that can move
    /// SOLID: Interface Segregation - Only movement-related functionality
    /// </summary>
    public interface IMovable {
        /// <summary>
        /// Current movement speed
        /// </summary>
        float MoveSpeed { get; set; }
        
        /// <summary>
        /// Jump force for the entity
        /// </summary>
        float JumpForce { get; set; }
        
        /// <summary>
        /// Whether the entity is currently grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Move the entity in a direction
        /// </summary>
        /// <param name="direction">Normalized movement direction</param>
        /// <param name="speed">Movement speed (uses MoveSpeed if -1)</param>
        void Move(Vector3 direction, float speed = -1f);
        
        /// <summary>
        /// Make the entity jump
        /// </summary>
        /// <param name="force">Jump force (uses JumpForce if -1)</param>
        void Jump(float force = -1f);
        
        /// <summary>
        /// Stop the entity's movement
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Event fired when entity starts moving
        /// </summary>
        System.Action OnMoveStart { get; set; }
        
        /// <summary>
        /// Event fired when entity stops moving
        /// </summary>
        System.Action OnMoveStop { get; set; }
        
        /// <summary>
        /// Event fired when entity jumps
        /// </summary>
        System.Action OnJump { get; set; }
        
        /// <summary>
        /// Event fired when entity lands
        /// </summary>
        System.Action OnLand { get; set; }
    }
}
