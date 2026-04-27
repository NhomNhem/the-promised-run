using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePromisedRun.Gameplay.Input {
    public class InputReader : MonoBehaviour {
        [field: SerializeField] public Vector2 MoveInput      { get; private set; }
        [field: SerializeField] public bool    IsJumpPressed   { get; private set; }
        [field: SerializeField] public bool    IsAttackPressed { get; private set; }

        public Vector3 MoveInput3D => new Vector3(MoveInput.x, 0, MoveInput.y);

        private PlayerInputActions _inputActions;
        private InputAction        _attackAction;

        private void OnEnable() {
            if (_inputActions == null) {
                _inputActions = new PlayerInputActions();

                _inputActions.Gameplay.Move.performed += OnMove;
                _inputActions.Gameplay.Move.canceled  += OnMove;

                _inputActions.Gameplay.Jump.started   += OnJump;
                _inputActions.Gameplay.Jump.canceled  += OnJump;

                // Attack action — find by name since it may not be in typed accessor yet
                _attackAction = _inputActions.asset.FindAction("Gameplay/Attack");
                if (_attackAction != null) {
                    _attackAction.started  += OnAttack;
                    _attackAction.canceled += OnAttack;
                } else {
                    Debug.LogWarning("[InputReader] 'Gameplay/Attack' action not found in InputActionAsset.");
                }
            }
            _inputActions.Enable();
        }

        private void OnDisable() {
            _inputActions.Disable();
        }

        public void OnMove(InputAction.CallbackContext ctx) =>
            MoveInput = ctx.ReadValue<Vector2>();

        public void OnJump(InputAction.CallbackContext ctx) {
            if (ctx.started)  IsJumpPressed = true;
            if (ctx.canceled) IsJumpPressed = false;
        }

        public void OnAttack(InputAction.CallbackContext ctx) {
            if (ctx.started)  IsAttackPressed = true;
            if (ctx.canceled) IsAttackPressed = false;
        }

        public void ConsumeJumpInput()   => IsJumpPressed   = false;
        public void ConsumeAttackInput() => IsAttackPressed = false;
    }
}
