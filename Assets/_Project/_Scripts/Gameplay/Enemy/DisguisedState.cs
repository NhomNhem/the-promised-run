using UnityEngine;
using ThePromisedRun.Core.FSM.Interfaces;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// MonsterPlant sits still, disguised as a normal plant.
    /// </summary>
    public class DisguisedState : IState {
        private readonly Animator _animator;
        private readonly int      _idleHash;

        public DisguisedState(Animator animator, int idleHash) {
            _animator = animator;
            _idleHash = idleHash;
        }

        public void OnEnter() {
            try { _animator.Play(_idleHash); }
            catch (System.Exception e) { Debug.LogWarning($"[DisguisedState] Anim error: {e.Message}"); }
        }

        public void OnUpdate()      { }
        public void OnFixedUpdate() { }
        public void OnExit()        { }
    }
}
