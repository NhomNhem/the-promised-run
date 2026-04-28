using UnityEngine;
using System.Collections.Generic;
using RaycastPro.Detectors;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Player {
    /// <summary>
    /// Concrete implementation of IEnemyDetector using RaycastPro RangeDetector.
    /// Extracted from PlayerController to satisfy SOLID Single Responsibility Principle.
    /// Req 2.2, 2.3, 10.1, 10.2, 10.3
    /// </summary>
    [RequireComponent(typeof(RangeDetector))]
    public class RaycastProEnemyDetector : MonoBehaviour, IEnemyDetector {

        #region Inspector Fields

        [SerializeField] private RangeDetector _rangeDetector;
        [SerializeField] private LayerMask _enemyLayer;

        #endregion

        #region Unity Lifecycle

        private void Awake() {
            // Req 10.2: Cache RangeDetector in Awake — no GetComponent per frame
            if (_rangeDetector == null) {
                _rangeDetector = GetComponent<RangeDetector>();
            }
        }

        #endregion

        #region IEnemyDetector

        /// <summary>
        /// Initializes the detector with the enemy layer mask from PlayerController.
        /// </summary>
        public void Initialize(LayerMask enemyLayer) {
            _enemyLayer = enemyLayer;
        }

        /// <summary>
        /// Returns the nearest enemy GameObject in range, or null if none found.
        /// Uses RangeDetector.NearestMember first, falls back to manual distance check.
        /// Req 10.1: Only component accessing RangeDetector.NearestMember for enemy detection.
        /// </summary>
        public GameObject GetNearestEnemy() {
            if (_rangeDetector == null || !_rangeDetector.Performed) return null;

            Collider nearest = _rangeDetector.NearestMember;
            if (nearest != null && IsEnemyLayer(nearest.gameObject.layer)) {
                return nearest.gameObject;
            }

            // Fallback: manual nearest search among enemy-layer colliders
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider col in _rangeDetector.DetectedColliders) {
                if (col == null || !IsEnemyLayer(col.gameObject.layer)) continue;

                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < nearestDistance) {
                    nearestDistance = distance;
                    nearestEnemy = col.gameObject;
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Returns all enemy GameObjects currently in detection range.
        /// Req 10.1: Only component accessing RangeDetector.DetectedColliders for enemy detection.
        /// </summary>
        public GameObject[] GetEnemiesInRange() {
            if (_rangeDetector == null || !_rangeDetector.Performed) {
                return new GameObject[0];
            }

            List<GameObject> enemies = new List<GameObject>();

            foreach (Collider col in _rangeDetector.DetectedColliders) {
                if (col != null && IsEnemyLayer(col.gameObject.layer)) {
                    enemies.Add(col.gameObject);
                }
            }

            return enemies.ToArray();
        }

        /// <summary>
        /// Returns true if player is surrounded: ≥3 enemies with at least one in front and one behind.
        /// Req 2.6: Returns false when enemies count < 3.
        /// </summary>
        public bool IsSurrounded() {
            GameObject[] enemies = GetEnemiesInRange();

            // Req 2.6: Must return false when fewer than 3 enemies
            if (enemies.Length < 3) return false;

            int enemiesInFront = 0;
            int enemiesBehind = 0;

            foreach (GameObject enemy in enemies) {
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, toEnemy);

                if (dot > 0.5f) {
                    enemiesInFront++;
                } else if (dot < -0.5f) {
                    enemiesBehind++;
                }
            }

            return enemiesInFront >= 1 && enemiesBehind >= 1;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks whether the given layer belongs to the enemy layer mask.
        /// </summary>
        private bool IsEnemyLayer(int layer) {
            return ((1 << layer) & _enemyLayer) != 0;
        }

        #endregion
    }
}
