using UnityEngine;
using UnityEngine.AI;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Enemy Chase state — pursues target using direct Rigidbody movement.
    /// Drives Walk animation via CrossFade.
    /// </summary>
    public class EnemyChaseState : IState {
        private readonly Enemy       _enemy;
        private readonly Rigidbody   _rb;
        private readonly NavMeshAgent _nav;
        private const float ChaseSpeed = 4f;

        public EnemyChaseState(Enemy enemy) {
            _enemy = enemy;
            _rb    = enemy.GetComponent<Rigidbody>();
            _nav   = enemy.GetComponent<NavMeshAgent>();
        }

        public void OnEnter() {
            _enemy.Animator?.SetBool("IsMoving", true);
            _enemy.Animator?.CrossFade("Walk", 0.1f, 0);
            // Disable NavMesh pathfinding — use direct movement
            if (_nav != null) {
                _nav.isStopped = true;
                _nav.ResetPath();
                _nav.updatePosition = false;
                _nav.updateRotation = false;
            }
        }

        public void OnUpdate() {
            if (!_enemy.HasTarget) return;

            Vector3 targetPos = _enemy.LastKnownTargetPosition;
            Vector3 toTarget  = targetPos - _enemy.transform.position;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;

            if (dist > 0.5f) {
                Vector3 dir = toTarget.normalized;
                if (_rb != null)
                    _rb.linearVelocity = new Vector3(dir.x * ChaseSpeed, _rb.linearVelocity.y, dir.z * ChaseSpeed);

                // Rotate toward target
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                _enemy.transform.rotation = Quaternion.Slerp(
                    _enemy.transform.rotation, targetRot, 10f * Time.deltaTime);

                _enemy.Animator?.SetBool("IsMoving", true);
            } else {
                if (_rb != null) _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                _enemy.Animator?.SetBool("IsMoving", false);
            }
        }

        public void OnFixedUpdate() { }

        public void OnExit() {
            if (_rb != null) _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            _enemy.Animator?.SetBool("IsMoving", false);
            // Re-enable NavMesh
            if (_nav != null) {
                _nav.updatePosition = true;
                _nav.updateRotation = true;
            }
        }
    }
}
