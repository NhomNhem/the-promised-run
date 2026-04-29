using UnityEngine;
using UnityEngine.AI;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Enemy;
using RaycastPro.Detectors;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Patrol state — enemy wanders via NavMesh random waypoints.
    /// Uses SightDetector (via EnemyDetector) to spot player.
    /// Falls back to Physics.OverlapSphere if no RaycastPro detector.
    /// </summary>
    public class PatrolState : EnemyStateBase {
        private const float PatrolRadius    = 6f;
        private const float WaypointReach   = 1.2f;  // distance to consider waypoint reached
        private const float WaitAtWaypoint  = 1.5f;  // seconds to wait before picking next
        private const float ScanInterval    = 0.25f;

        private Vector3 _spawnCenter;
        private Vector3 _currentWaypoint;
        private bool    _hasWaypoint;
        private float   _waitTimer;
        private bool    _waiting;
        private float   _scanTimer;
        private EnemyDetector _detector;
        private NavMeshAgent  _navAgent;

        public PatrolState(IEnemyEntity enemyEntity, IEnemyAI aiController)
            : base(enemyEntity, aiController, EnemyAIState.Patrol) { }

        protected override void OnEnter() {
            _spawnCenter  = EnemyEntity.GameObject.transform.position;
            _hasWaypoint  = false;
            _waiting      = false;
            _waitTimer    = 0f;
            _scanTimer    = 0f;
            _detector     = EnemyEntity.GameObject.GetComponent<EnemyDetector>();
            _navAgent     = EnemyEntity.GameObject.GetComponent<NavMeshAgent>();
            string isOnNavStr = _navAgent != null ? _navAgent.isOnNavMesh.ToString() : "N/A";
            Debug.Log($"[PatrolState] Enter - spawn={_spawnCenter} navAgentPresent={_navAgent != null} isOnNavMesh={isOnNavStr}");
            // Animation
            var anim = EnemyEntity.GameObject.GetComponentInChildren<Animator>();
            anim?.SetBool("IsMoving", false);
            PickNextWaypoint();
        }

        protected override void OnExit() {
            if (_navAgent != null) _navAgent.isStopped = true;
        }

        protected override void OnUpdate() {
            // Scan for player
            _scanTimer += Time.deltaTime;
            if (_scanTimer >= ScanInterval) {
                _scanTimer = 0f;
                ScanForPlayer();
            }

            if (HasTarget()) {
                AIController.ChangeState(EnemyAIState.Chase);
                return;
            }

            // Patrol movement
            if (_waiting) {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= WaitAtWaypoint) {
                    _waiting = false;
                    PickNextWaypoint();
                }
                return;
            }

            if (_hasWaypoint) {
                float dist = Vector3.Distance(
                    EnemyEntity.GameObject.transform.position, _currentWaypoint);

                if (dist <= WaypointReach) {
                    // Reached waypoint — wait then pick next
                    EnemyEntity.StopMovement();
                    var anim = EnemyEntity.GameObject.GetComponentInChildren<Animator>();
                    anim?.SetBool("IsMoving", false);
                    _waiting   = true;
                    _waitTimer = 0f;
                } else {
                    EnemyEntity.MoveTowards(_currentWaypoint);
                    var anim = EnemyEntity.GameObject.GetComponentInChildren<Animator>();
                    anim?.SetBool("IsMoving", true);
                }
            }
        }

        private void PickNextWaypoint() {
            // Try NavMesh random point
            for (int i = 0; i < 5; i++) {
                Vector3 randomDir = Random.insideUnitSphere * PatrolRadius;
                randomDir.y = 0f;
                Vector3 candidate = _spawnCenter + randomDir;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, PatrolRadius, NavMesh.AllAreas)) {
                    _currentWaypoint = hit.position;
                    _hasWaypoint     = true;
                    Debug.Log($"[PatrolState] PickNextWaypoint - sampled waypoint: {hit.position}");
                    return;
                }
            }

            // Fallback: move to random direction
            Vector3 fallback = _spawnCenter + Random.insideUnitSphere * PatrolRadius;
            fallback.y = EnemyEntity.GameObject.transform.position.y;
            _currentWaypoint = fallback;
            _hasWaypoint     = true;
            Debug.Log($"[PatrolState] PickNextWaypoint - fallback waypoint: {_currentWaypoint}");
        }

        private void ScanForPlayer() {
            IDamageable player = null;

            // Force update SightDetector before checking
            if (_detector != null) {
                var sightDet = _detector.GetComponent<RaycastPro.Detectors.SightDetector>();
                if (sightDet != null) {
                    sightDet.Cast();
                }
                player = _detector.GetPlayerInSight();
            }
            if (player != null) Debug.Log($"[PatrolState] Detector saw player at {((MonoBehaviour)player).transform.position}");

            // Fallback: Physics.OverlapSphere
            if (player == null) {
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer == -1) {
                    // Try common player layer names
                    string[] layerNames = { "Player", "PlayerCharacter", "Default" };
                    foreach (string name in layerNames) {
                        int layer = LayerMask.NameToLayer(name);
                        if (layer != -1) {
                            playerLayer = layer;
                            break;
                        }
                    }
                }
                
                if (playerLayer != -1) {
                    var cols = Physics.OverlapSphere(
                        EnemyEntity.GameObject.transform.position,
                        EnemyEntity.DetectionRadius,
                        1 << playerLayer);
                    foreach (var col in cols) {
                        var dmg = col.GetComponentInParent<IDamageable>();
                        if (dmg != null && dmg.IsAlive) { player = dmg; break; }
                    }
                    Debug.Log($"[PatrolState] Fallback scan - found player: {player != null}");
                } else {
                    // No Player layer - log warning
                    Debug.LogWarning($"[PatrolState] No Player layer found! Layers: Default={LayerMask.NameToLayer("Default")}, Player={LayerMask.NameToLayer("Player")}");
                }
            }

            if (player != null) {
                Debug.Log($"[PatrolState] ScanForPlayer - acquiring target and notifying AIController for {EnemyEntity.GameObject.name}");
                AIController.SetTarget(player);
            }
        }
    }
}
