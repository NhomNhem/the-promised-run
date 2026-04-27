using UnityEngine;

namespace ThePromisedRun.Gameplay.Enemy {
    public class EnemyDeathState : EnemyBaseState {
        private const float DestroyDelay = 3f;
        private float _timer;

        public EnemyDeathState(EnemyController enemy, Animator animator)
            : base(enemy, animator) { }

        public override void OnEnter() {
            _timer = 0f;
            _enemy.Rb.linearVelocity = Vector3.zero;
            _enemy.Rb.isKinematic    = true;
            _animator.SetTrigger("Die");

            // Disable colliders so player can walk through corpse
            foreach (var col in _enemy.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        public override void OnUpdate() {
            _timer += Time.deltaTime;
            if (_timer >= DestroyDelay)
                Object.Destroy(_enemy.gameObject);
        }
    }
}
