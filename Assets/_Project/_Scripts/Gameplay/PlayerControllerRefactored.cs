using UnityEngine;
using System.Linq;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Input;

namespace ThePromisedRun.Gameplay {
    /// <summary>
    /// Refactored PlayerController following SOLID principles
    /// SOLID: Single Responsibility - Only handles input processing and movement coordination
    /// </summary>
    public class PlayerControllerRefactored : MonoBehaviour {
        [Header("Components")]
        [SerializeField] private Player player;
        [SerializeField] private InputReader input;
        [SerializeField] private Transform visual;
        [SerializeField] private Transform detector;
        
        [Header("Ground Detection")]
        [SerializeField] private RaycastPro.Detectors.RangeDetector groundDetector;
        
        [Header("RaycastPro Detection")]
        [SerializeField] private RaycastPro.Detectors.RangeDetector enemyDetector;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float enemyDetectionRadius = 10f;
        
        // Private state
        private int _groundContacts;
        private bool _isGrounded;
        
        public Player Player => player;
        public bool IsGrounded => _isGrounded;
        public Transform Visual => visual;
        
        private void Awake() {
            // Get components
            if (player == null) player = GetComponent<Player>();
            if (input == null) input = GetComponent<InputReader>();
            
            // Find visual if not assigned
            if (visual == null) {
                visual = Enumerable.Range(0, transform.childCount)
                    .Select(i => transform.GetChild(i))
                    .FirstOrDefault(c => c.name == "Visual");
            }
            
            // Find detector if not assigned
            if (detector == null) {
                detector = Enumerable.Range(0, transform.childCount)
                    .Select(i => transform.GetChild(i))
                    .FirstOrDefault(c => c.name == "Detector");
            }
            
            // Get ground detector
            if (detector != null) {
                groundDetector = detector.GetComponentInChildren<RaycastPro.Detectors.RangeDetector>();
            }
            
            // Setup player references
            if (player != null) {
                player.SetGrounded(_isGrounded);
            }
        }
        
        private void Update() {
            HandleInput();
            CheckGround();
        }
        
        private void FixedUpdate() {
            // Ground detection updates are handled in CheckGround()
        }
        
        #region Input Handling
        private void HandleInput() {
            if (player == null || !player.IsAlive) return;
            
            // Movement input
            Vector2 moveInput = input.MoveInput;
            if (moveInput.sqrMagnitude > 0.01f) {
                Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                player.Move(moveDir);
            } else {
                player.Stop();
            }
            
            // Jump input
            if (input.IsJumpPressed && _isGrounded) {
                player.Jump();
                input.ConsumeJumpInput();
            }
            
            // Attack input
            if (input.IsAttackPressed) {
                player.Attack();
                input.ConsumeAttackInput();
            }
        }
        #endregion
        
        #region Ground Detection
        private void CheckGround() {
            bool wasGrounded = _isGrounded;
            _isGrounded = groundDetector != null 
                ? groundDetector.Performed 
                : _groundContacts > 0;
            
            // Notify player of ground state change
            if (player != null && wasGrounded != _isGrounded) {
                player.SetGrounded(_isGrounded);
            }
        }
        
        private void OnCollisionEnter(Collision collision) {
            _groundContacts++;
        }
        
        private void OnCollisionExit(Collision collision) {
            _groundContacts--;
        }
        #endregion
        
        #region RaycastPro Detection Methods
        /// <summary>
        /// Get nearest enemy using RaycastPro RangeDetector
        /// </summary>
        public GameObject GetNearestEnemy() {
            if (enemyDetector == null || !enemyDetector.Performed) return null;
            
            var nearest = enemyDetector.NearestMember;
            if (nearest != null && IsEnemyLayer(nearest.gameObject.layer)) {
                return nearest.gameObject;
            }
            
            // Fallback: check all detected colliders
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var collider in enemyDetector.DetectedColliders) {
                if (collider != null && IsEnemyLayer(collider.gameObject.layer)) {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < nearestDistance) {
                        nearestDistance = distance;
                        nearestEnemy = collider.gameObject;
                    }
                }
            }
            
            return nearestEnemy;
        }
        
        /// <summary>
        /// Get all enemies in detection range
        /// </summary>
        public GameObject[] GetEnemiesInRange() {
            if (enemyDetector == null || !enemyDetector.Performed) 
                return new GameObject[0];
            
            var enemies = new System.Collections.Generic.List<GameObject>();
            
            foreach (var collider in enemyDetector.DetectedColliders) {
                if (collider != null && IsEnemyLayer(collider.gameObject.layer)) {
                    enemies.Add(collider.gameObject);
                }
            }
            
            return enemies.ToArray();
        }
        
        /// <summary>
        /// Check if player is surrounded by enemies
        /// </summary>
        public bool IsSurrounded() {
            var enemies = GetEnemiesInRange();
            if (enemies.Length < 3) return false;
            
            int enemiesInFront = 0;
            int enemiesBehind = 0;
            
            foreach (var enemy in enemies) {
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, toEnemy);
                
                if (dotProduct > 0.5f) enemiesInFront++;
                else if (dotProduct < -0.5f) enemiesBehind++;
            }
            
            return enemiesInFront >= 1 && enemiesBehind >= 1;
        }
        
        private bool IsEnemyLayer(int layer) {
            return ((1 << layer) & enemyLayer) != 0;
        }
        #endregion
        
        #region Public Methods for External Systems
        /// <summary>
        /// Get player's current position
        /// </summary>
        public Vector3 GetPosition() {
            return transform.position;
        }
        
        /// <summary>
        /// Get player's forward direction
        /// </summary>
        public Vector3 GetForwardDirection() {
            return transform.forward;
        }
        
        /// <summary>
        /// Check if player can attack in current direction
        /// </summary>
        public bool CanAttackInDirection(Vector3 direction) {
            if (player == null || !player.CanAttack) return false;
            
            // Check if there's an enemy in attack range in the direction
            var enemies = GetEnemiesInRange();
            foreach (var enemy in enemies) {
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(direction, toEnemy);
                if (dotProduct > 0.7f) { // Enemy is in front cone
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
