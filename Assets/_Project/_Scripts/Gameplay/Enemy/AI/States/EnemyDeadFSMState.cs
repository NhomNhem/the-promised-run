using UnityEngine;
using System.Collections;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Enemy Dead state — plays Death animation, disables physics, then despawns the GameObject.
    /// Terminal state — no transitions out.
    /// </summary>
    public class EnemyDeadFSMState : IState {
        private readonly Enemy _enemy;

        public EnemyDeadFSMState(Enemy enemy) {
            _enemy = enemy;
        }

        public void OnEnter() {
            _enemy.StopMovement();
            _enemy.Animator?.SetBool("IsMoving", false);
            // Single source of truth for death animation — Enemy.HandleDeath() must NOT call SetTrigger("Death")
            _enemy.Animator?.CrossFade("Death", 0.1f, 0);

            // Disable NavMeshAgent and collider
            var nav = _enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.enabled = false;

            var col = _enemy.GetComponent<UnityEngine.CapsuleCollider>();
            if (col != null) col.enabled = false;

            var rb = _enemy.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // Schedule despawn after death animation finishes
            _enemy.StartCoroutine(DespawnAfterDeath());
        }

        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public void OnExit() { }

        /// <summary>
        /// Waits for the Death animation to finish, then disables the GameObject.
        /// Falls back to a 2-second delay if the animation length cannot be read.
        /// </summary>
        private IEnumerator DespawnAfterDeath() {
            float deathDuration = 2f;

            // Wait one frame so CrossFade has time to transition into the Death state
            yield return null;

            if (_enemy.Animator != null) {
                AnimatorStateInfo info = _enemy.Animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("Death") && info.length > 0f) {
                    deathDuration = info.length;
                }
            }

            yield return new WaitForSeconds(deathDuration);

            _enemy.gameObject.SetActive(false);
        }
    }
}
