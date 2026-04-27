using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// MonsterPlant chases the player using Rigidbody.linearVelocity.
    /// </summary>
    public class ChaseState : IState {
        private readonly Animator         _animator;
        private readonly Rigidbody        _rb;
        private readonly PlayerController _player;
        private readonly Transform        _transform;
        private readonly float            _speed;
        private readonly int              _runHash;

        public ChaseState(Animator animator, Rigidbody rb, PlayerController player,
                          Transform transform, float speed, int runHash) {
            _animator  = animator;
            _rb        = rb;
            _player    = player;
            _transform = transform;
            _speed     = speed;
            _runHash   = runHash;
        }

        public void OnEnter() {
            try { _animator.Play(_runHash); }
            catch (System.Exception e) { Debug.LogWarning($"[ChaseState] Anim error: {e.Message}"); }
        }

        public void OnUpdate() {
            if (_player == null) return;

            // Rotate to face player
            Vector3 dir = (_player.transform.position - _transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f) {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRot, 10f * Time.deltaTime);
            }
        }

        public void OnFixedUpdate() {
            if (_player == null) return;

            Vector3 dir = (_player.transform.position - _transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f) {
                Vector3 velocity = dir.normalized * _speed;
                _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);
            }
        }

        public void OnExit() {
            // Stop horizontal movement when leaving chase
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        }
    }
}
