using UnityEngine;
using RaycastPro.Detectors;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Bridges RaycastPro detectors to the Enemy AI system.
    /// Attach alongside SightDetector and RangeDetector.
    /// </summary>
    public class EnemyDetector : MonoBehaviour {
        [Header("RaycastPro Detectors")]
        [SerializeField] private SightDetector _sightDetector;
        [SerializeField] private RangeDetector _attackRangeDetector;

        private void Awake() {
            if (_sightDetector == null)
                _sightDetector = GetComponent<SightDetector>();
            if (_attackRangeDetector == null)
                _attackRangeDetector = GetComponent<RangeDetector>();
        }

        /// <summary>Returns first IDamageable in sight cone, or null.</summary>
        public IDamageable GetPlayerInSight() {
            if (_sightDetector == null || !_sightDetector.Performed) return null;
            foreach (Collider col in _sightDetector.DetectedColliders) {
                if (col == null) continue;
                var dmg = col.GetComponentInParent<IDamageable>();
                if (dmg != null && dmg.IsAlive) return dmg;
            }
            return null;
        }

        /// <summary>Returns true if any player is within attack range.</summary>
        public bool IsPlayerInAttackRange() {
            if (_attackRangeDetector == null) return false;
            return _attackRangeDetector.Performed &&
                   _attackRangeDetector.DetectedColliders.Count > 0;
        }

        /// <summary>Returns nearest player collider in attack range, or null.</summary>
        public IDamageable GetPlayerInAttackRange() {
            if (_attackRangeDetector == null || !_attackRangeDetector.Performed) return null;
            Collider nearest = _attackRangeDetector.NearestMember;
            if (nearest != null) {
                var dmg = nearest.GetComponentInParent<IDamageable>();
                if (dmg != null && dmg.IsAlive) return dmg;
            }
            return null;
        }
    }
}
