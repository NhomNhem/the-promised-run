using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePromisedRun.Gameplay.Input {
    public class InputReader : MonoBehaviour {
        [field: SerializeField] public Vector2 MoveInput { get; private set; }
        [field: SerializeField] public bool IsJumpPressed { get; private set; }

        public Vector3 MoveInput3D => new Vector3(MoveInput.x, 0, MoveInput.y);
        
        private PlayerInputActions _inputActions;

        private void OnEnable() {
            if (_inputActions == null) {
                _inputActions = new PlayerInputActions();

                _inputActions.Gameplay.Move.performed += OnMove;
                _inputActions.Gameplay.Move.canceled += OnMove;

                _inputActions.Gameplay.Jump.started += OnJump;
                _inputActions.Gameplay.Jump.canceled += OnJump;
            }
            
            _inputActions.Enable();
        }

        private void OnDisable() {
            _inputActions.Disable();
        }

        public void OnMove(InputAction.CallbackContext context) {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.started) IsJumpPressed = true;
            if (context.canceled) IsJumpPressed = false;
        }

        public void ConsumeJumpInput() => IsJumpPressed = false;
    }
}