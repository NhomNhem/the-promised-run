using ThePromisedRune.Core.FSM.Interfaces;
using UnityEngine;

namespace ThePromisedRun.Gameplay.States {
    public abstract class BaseState : IState{
        protected readonly PlayerController _playerController; 
        protected readonly Animator _animator;
        
        protected BaseState(PlayerController playerController, Animator animator) {
            _playerController = playerController;
            _animator = animator;
        }
        
        
        public void OnEnter() {
        }
        public void OnUpdate() {
        }
        public void OnFixedUpdate() {
        }
        public void OnExit() {
        }
    }

    public class PlayerController { }
}