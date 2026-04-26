using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePromisedRun.Gameplay.Input {
    public class InputReader : MonoBehaviour {
        [field: SerializeField] public Vector2 MoveInput { get; private set; }
        [field: SerializeField] public bool IsJumpPressed { get; private set; }

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