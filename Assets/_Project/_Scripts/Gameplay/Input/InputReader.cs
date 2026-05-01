using UnityEngine;
using UnityEngine.InputSystem;

namespace ThePromisedRun.Gameplay.Input {
    public class InputReader : MonoBehaviour {
        [field: SerializeField] public Vector2 MoveInput      { get; private set; }
        [field: SerializeField] public bool    IsJumpPressed   { get; private set; }
        [field: SerializeField] public bool    IsJumpHeld      { get; private set; } // true while jump button held
        [field: SerializeField] public bool    IsAttackPressed { get; private set; }
        [field: SerializeField] public bool    IsDashPressed   { get; private set; }
        [field: SerializeField] public bool    IsParryPressed  { get; private set; }
        [field: SerializeField] public bool    IsPausePressed  { get; private set; }

        /// <summary>True for 6 frames after jump was pressed (jump buffer).</summary>
        public bool HasJumpBuffer { get; private set; }

        public Vector3 MoveInput3D => new Vector3(MoveInput.x, 0, MoveInput.y);

        private const float JumpBufferTime = 6f / 60f; // 6 frames at 60fps
        private float _jumpBufferTimer;

        private PlayerInputActions _inputActions;
        private InputAction        _attackAction;
        private InputAction        _dashAction;
        private InputAction        _pauseAction;

        private void Update() {
            if (_jumpBufferTimer > 0f) {
                _jumpBufferTimer -= Time.deltaTime;
                HasJumpBuffer = _jumpBufferTimer > 0f;
            }
        }

        private void OnEnable() {
            if (_inputActions == null) {
                _inputActions = new PlayerInputActions();

                _inputActions.Gameplay.Move.performed += OnMove;
                _inputActions.Gameplay.Move.canceled  += OnMove;

                _inputActions.Gameplay.Jump.started   += OnJump;
                _inputActions.Gameplay.Jump.canceled  += OnJump;

                _attackAction = _inputActions.asset.FindAction("Gameplay/Attack");
                if (_attackAction != null) {
                    _attackAction.started  += OnAttack;
                    _attackAction.canceled += OnAttack;
                } else {
                    Debug.LogWarning("[InputReader] 'Gameplay/Attack' action not found.");
                }

                _dashAction = _inputActions.asset.FindAction("Gameplay/Dash");
                if (_dashAction != null) {
                    _dashAction.started  += OnDash;
                    _dashAction.canceled += OnDash;
                }

                var parryAction = _inputActions.asset.FindAction("Gameplay/Parry");
                if (parryAction != null) {
                    parryAction.started  += ctx => { if (ctx.started) IsParryPressed = true; };
                    parryAction.canceled += ctx => IsParryPressed = false;
                }

                _pauseAction = _inputActions.asset.FindAction("Gameplay/Pause");
                if (_pauseAction != null) {
                    _pauseAction.started  += OnPause;
                    _pauseAction.canceled += OnPause;
                } else {
                    Debug.LogWarning("[InputReader] 'Gameplay/Pause' action not found.");
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
            if (ctx.started) {
                IsJumpPressed    = true;
                IsJumpHeld       = true;
                _jumpBufferTimer = JumpBufferTime;
                HasJumpBuffer    = true;
            }
            if (ctx.canceled) {
                IsJumpPressed = false;
                IsJumpHeld    = false;
            }
        }

        public void OnAttack(InputAction.CallbackContext ctx) {
            if (ctx.started)  IsAttackPressed = true;
            if (ctx.canceled) IsAttackPressed = false;
        }

        public void OnDash(InputAction.CallbackContext ctx) {
            if (ctx.started)  IsDashPressed = true;
            if (ctx.canceled) IsDashPressed = false;
        }

        public void OnPause(InputAction.CallbackContext ctx) {
            if (ctx.started)  IsPausePressed = true;
            if (ctx.canceled) IsPausePressed = false;
        }

        public void ConsumeJumpInput() {
            IsJumpPressed    = false;
            HasJumpBuffer    = false;
            _jumpBufferTimer = 0f;
        }
        public void ConsumeAttackInput() => IsAttackPressed = false;
        public void ConsumeDashInput()   => IsDashPressed   = false;
        public void ConsumeParryInput()  => IsParryPressed  = false;
        public void ConsumePauseInput()  => IsPausePressed  = false;
    }
}
