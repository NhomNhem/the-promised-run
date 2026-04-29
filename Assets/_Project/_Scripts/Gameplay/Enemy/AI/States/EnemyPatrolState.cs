using UnityEngine;
using UnityEngine.AI;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Enemy Patrol state — wanders random waypoints near spawn.
    /// Uses direct Rigidbody movement (no NavMesh pathfinding) for reliability.
    /// NavMeshAgent is used only for obstacle avoidance if available.
    /// </summary>
    public class EnemyPatrolState : IState {
        private readonly Enemy      _enemy;
        private readonly NavMeshAgent _nav;
        private readonly Rigidbody  _rb;

        private const float PatrolRadius  = 5f;
        private const float WaypointReach = 1.5f;
        private const float WaitTime      = 1.5f;
        private const float MoveSpeed     = 2.5f;

        private Vector3 _spawnPos;
        private Vector3 _waypoint;
        private bool    _hasWaypoint;
        private bool    _waiting;
        private float   _waitTimer;

        public EnemyPatrolState(Enemy enemy) {
            _enemy = enemy;
            _nav   = enemy.GetComponent<NavMeshAgent>();
            _rb    = enemy.GetComponent<Rigidbody>();
        }

        public void OnEnter() {
            _spawnPos    = _enemy.transform.position;
            _hasWaypoint = false;
            _waiting     = false;
            _waitTimer   = 0f;
            _enemy.Animator?.SetBool("IsMoving", false);
            _enemy.Animator?.CrossFade("Idle", 0.1f, 0);

            // Disable NavMeshAgent pathfinding — use direct movement
            if (_nav != null) {
                _nav.isStopped = true;
                _nav.ResetPath();
                _nav.updatePosition = false;
                _nav.updateRotation = false;
            }

            PickWaypoint();
        }

        public void OnUpdate() {
            if (_waiting) {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= WaitTime) {
                    _waiting = false;
                    PickWaypoint();
                }
                return;
            }

            if (!_hasWaypoint) return;

            Vector3 toWaypoint = _waypoint - _enemy.transform.position;
            toWaypoint.y = 0f;
            float dist = toWaypoint.magnitude;

            if (dist <= WaypointReach) {
                // Reached — stop and wait
                if (_rb != null) _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                _enemy.Animator?.SetBool("IsMoving", false);
                _enemy.Animator?.CrossFade("Idle", 0.1f, 0);
                _waiting   = true;
                _waitTimer = 0f;
            } else {
                // Move directly toward waypoint
                Vector3 dir = toWaypoint.normalized;
                if (_rb != null) {
                    _rb.linearVelocity = new Vector3(dir.x * MoveSpeed, _rb.linearVelocity.y, dir.z * MoveSpeed);
                }
                // Rotate toward movement direction
                if (dir.sqrMagnitude > 0.01f) {
                    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    _enemy.transform.rotation = Quaternion.Slerp(
                        _enemy.transform.rotation, targetRot, 8f * Time.deltaTime);
                }
                _enemy.Animator?.SetBool("IsMoving", true);
                _enemy.Animator?.CrossFade("Walk", 0.15f, 0);
            }
        }

        public void OnFixedUpdate() { }

        public void OnExit() {
            if (_rb != null) _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            // Re-enable NavMeshAgent for chase/attack
            if (_nav != null) {
                _nav.updatePosition = true;
                _nav.updateRotation = true;
                _nav.isStopped = false;
            }
        }

        private void PickWaypoint() {
            // Pick random point within patrol radius, staying on same Y level
            for (int i = 0; i < 10; i++) {
                Vector2 rand2D = Random.insideUnitCircle * PatrolRadius;
                Vector3 candidate = _spawnPos + new Vector3(rand2D.x, 0f, rand2D.y);

                // Clamp to corridor bounds (X: -5 to 5)
                candidate.x = Mathf.Clamp(candidate.x, -5f, 5f);
                candidate.y = _spawnPos.y;

                // Verify NavMesh exists at candidate (optional check)
                NavMeshHit hit;
                if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas)) {
                    _waypoint    = new Vector3(hit.position.x, _spawnPos.y, hit.position.z);
                    _hasWaypoint = true;
                    return;
                }
            }
            // Fallback: move to opposite side of spawn
            _waypoint    = _spawnPos + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
            _waypoint.x  = Mathf.Clamp(_waypoint.x, -5f, 5f);
            _waypoint.y  = _spawnPos.y;
            _hasWaypoint = true;
        }
    }
}
