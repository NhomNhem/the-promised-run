using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Plays death animation, disables colliders, destroys the GameObject after 2 seconds.
    /// </summary>
    public class DeadState : IState {
        private static readonly float DestroyDelay = 2f;

        private readonly Animator    _animator;
        private readonly int         _dieHash;
        private readonly GameObject  _gameObject;

        public DeadState(Animator animator, int dieHash, GameObject gameObject) {
            _animator   = animator;
            _dieHash    = dieHash;
            _gameObject = gameObject;
        }

        public void OnEnter() {
            try { _animator.Play(_dieHash); }
            catch (System.Exception e) { Debug.LogWarning($"[DeadState] Anim error: {e.Message}"); }

            // Disable all colliders
            foreach (Collider col in _gameObject.GetComponentsInChildren<Collider>())
                col.enabled = false;

            // Stop physics movement
            Rigidbody rb = _gameObject.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic    = true;
            }

            Object.Destroy(_gameObject, DestroyDelay);
        }

        public void OnUpdate()      { }
        public void OnFixedUpdate() { }
        public void OnExit()        { }
    }
}
