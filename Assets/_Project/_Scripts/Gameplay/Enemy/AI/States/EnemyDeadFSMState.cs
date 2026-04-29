using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Enemy Dead state — plays Death animation, disables physics.
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
            _enemy.Animator?.CrossFade("Death", 0.1f, 0);

            // Disable NavMeshAgent and collider
            var nav = _enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.enabled = false;

            var col = _enemy.GetComponent<UnityEngine.CapsuleCollider>();
            if (col != null) col.enabled = false;

            var rb = _enemy.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public void OnExit() { }
    }
}
