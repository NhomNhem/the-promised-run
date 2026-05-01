using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy.AI.States {
    /// <summary>
    /// Enemy Hit state — pauses movement while the Hit animation plays.
    /// Automatically exits after HitDuration seconds, returning to Chase (if target
    /// exists) or Patrol (if no target) via transitions wired in EnemyBrain.
    /// </summary>
    public class EnemyHitState : IState {
        private readonly Enemy _enemy;

        /// <summary>Duration to stay in Hit state — should match the Hit animation clip length.</summary>
        private const float HitDuration = 0.4f;

        private float _timer;

        /// <summary>True once the hit stun duration has elapsed — used by EnemyBrain transition predicates.</summary>
        public bool HitComplete => _timer >= HitDuration;

        public EnemyHitState(Enemy enemy) {
            _enemy = enemy;
        }

        public void OnEnter() {
            _timer = 0f;
            _enemy.StopMovement();
            _enemy.Animator?.SetBool("IsMoving", false);
            // Short crossfade so the hit reaction blends in quickly
            _enemy.Animator?.CrossFade("Hit", 0.05f, 0);
        }

        public void OnUpdate() {
            _timer += Time.deltaTime;
        }

        public void OnFixedUpdate() { }

        public void OnExit() { }
    }
}
