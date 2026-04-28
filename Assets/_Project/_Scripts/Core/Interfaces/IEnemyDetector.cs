using UnityEngine;

namespace ThePromisedRun.Core.Interfaces {
    /// <summary>
    /// Interface for enemy detection logic
    /// SOLID: Interface Segregation - Separates detection from PlayerController
    /// SOLID: Dependency Inversion - PlayerController depends on abstraction, not RaycastPro directly
    /// </summary>
    public interface IEnemyDetector {
        /// <summary>
        /// Returns the nearest enemy GameObject in range, or null if none found
        /// </summary>
        GameObject GetNearestEnemy();

        /// <summary>
        /// Returns all enemy GameObjects currently in detection range
        /// </summary>
        GameObject[] GetEnemiesInRange();

        /// <summary>
        /// Returns true if player is surrounded (≥3 enemies with at least one in front and one behind)
        /// </summary>
        bool IsSurrounded();

        /// <summary>
        /// Initializes the detector with the enemy layer mask
        /// </summary>
        /// <param name="enemyLayer">LayerMask used to filter enemy colliders</param>
        void Initialize(LayerMask enemyLayer);
    }
}
