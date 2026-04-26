using ThePromisedRun.Core.FSM.Interfaces;
using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public abstract class BaseState : IState{
        protected readonly PlayerController _playerController; 
        protected readonly Animator _animator;
        
        protected BaseState(PlayerController playerController, Animator animator) {
            _playerController = playerController;
            _animator = animator;
        }
        
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnExit() { }
    }
}