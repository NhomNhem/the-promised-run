using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BLINK.Controller
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class TopDownWASDController : MonoBehaviour
    {
        // REFERENCES
        public Animator _anim;
        public CharacterController _characterController;

        // CAMERA
        public Camera playerCamera;
        public bool cameraEnabled = true;
        public bool initCameraOnSpawn = true;
        public string cameraName = "Main Camera";
        public Vector3 cameraPositionOffset = new Vector3(0, 10, 0);
        public Vector3 cameraRotationOffset = new Vector3(45, 0, 0);
        public float cameraDampTime = 0.1f;
        public float offsetDampTime = 0.25f;
        public float maxOffset = 0.3f;
        public bool isDraggable = true;
        public float minCameraHeight = 3,
            maxCameraHeight = 15,
            minCameraVertical = 0.5f,
            maxCameraVertical = 0.5f,
            cameraZoomSpeed = 15,
            cameraZoomPower = 5;
        private float _currentCameraHeight, _cameraHeightTarget, _currentCameraVertical, _cameraVerticalTarget;
        private Vector3 _forward;
        private Vector3 _cameraVelocity;
        private Vector3 _dampedCameraPosition;
        private Vector2 _dampedOffsetVector;
        private Vector2 _currentDistanceVectorVelocity;
        private float _dragAngle;
        private bool _isDragging;
        
        // NAVIGATION
        public bool movementEnabled = true;
        public float jumpHeight = 4;
        public float gravity = 10;
        public float moveSpeed = 5;
        public float dashDistance = 4f;
        public float dashDuration = 0.15f;
        [Range(0.1f, 1f)] public float dashSpeedMultiplier = 0.65f;
        public float dashCooldown = 0.5f;
        public float attackComboResetTime = 0.6f;
        [Range(0f, 1f)] public float attackReleaseNormalizedTime = 1f;
        public int attackAnimatorLayer = 1;
        
        // INPUT
        public KeyCode moveUpKey = KeyCode.W,
            moveDownKey = KeyCode.S,
            moveLeftKey = KeyCode.A,
            moveRightKey = KeyCode.D,
            jumpKey = KeyCode.Space,
            dashKey = KeyCode.LeftShift;

        public enum MovementInputType
        {
            Keyboard = 0
        }
        public MovementInputType movementInputType;

        // IK
        public float bodyWeightIK = 0.5f;
        public float headWeightIK = 1.0f;
        public float dampSmoothTimeIK = 0.4f;
        public float dampSmoothTimeRotation = 0.25f;
        
        // ANIMATOR 
        static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
        static readonly int VerticalHash = Animator.StringToHash("Vertical");
        static readonly int FallingHash = Animator.StringToHash("Falling");
        static readonly int JumpHash = Animator.StringToHash("Jump");
        static readonly int DashHash = Animator.StringToHash("Dash");
        static readonly int Attack1Hash = Animator.StringToHash("Attack1");
        static readonly int Attack2Hash = Animator.StringToHash("Attack2");
        static readonly int Attack3Hash = Animator.StringToHash("Attack3");
        public float animatorSmoothTime = 0.15f;

        // OTHER
        private float _deltaAngle;
        private Vector3 _displacement;
        private bool _isJumping;
        private float _deltaAngleVelocity;
        private float _angularVelocity;
        private float _verticalSpeed;
        private float _forwardF = 180.0f;
        private float _lookAngle = 180.0f;
        private float _actualMoveSpeed;
        private float _targetAngle = 180.0f;
        private float _rotationAngle = 180.0f;
        private float _targetRotationAngle = 180.0f;
        private Vector2? _targetPosition;
        private bool _isDashing;
        private Vector3 _dashDirection;
        private float _dashSpeed;
        private float _dashRemainingDistance;
        private float _dashCooldownTimer;
        private const float AirControlMultiplier = 0.75f;
        private const int MaxAttackComboStep = 3;
        private int _attackComboStep;
        private float _attackComboTimer;
        private int _bufferedAttackInputs;
        private int _attackWarmupFrames;

        private void Awake()
        {
            _actualMoveSpeed = moveSpeed;
            InitReferences();
            
            if (cameraEnabled)
            {
                InitCameraValues();
                InitCamera();
            }
        }

        void Update()
        {
            if (movementEnabled) HandleMovement();
            HandleAttack();
            if (cameraEnabled)  CameraLogic();
        }

        private void LateUpdate()
        {
            if (cameraEnabled) LateCameraUpdate();
        }

        private void InitReferences()
        {
            _anim = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();
        }

        private void LateCameraUpdate()
        {
            if (playerCamera == null) return;

            if (isDraggable)
            {
                if (IsMiddleMousePressedThisFrame())
                {
                    Vector3 point = GetPoint();
                    if (Vector3.Distance(point, transform.position) > 3f)
                    {
                        Vector3 targetCameraPosition = transform.position - _forward * _currentCameraHeight;

                        _isDragging = true;

                        _dragAngle =
                            Mathf.Atan2(transform.position.z - point.z, point.x - transform.position.x) *
                            Mathf.Rad2Deg + 90;

                        _dampedOffsetVector =
                            new Vector2(_dampedCameraPosition.x - targetCameraPosition.x + _dampedOffsetVector.x,
                                _dampedCameraPosition.z - targetCameraPosition.z + _dampedOffsetVector.y);
                    }
                }
                else if (IsMiddleMousePressed())
                {
                    Vector3 point = GetPoint();
                    if (_isDragging && Vector3.Distance(point, transform.position) > 0.25f)
                    {
                        float dragAngle =
                            Mathf.Atan2(transform.position.z - point.z, point.x - transform.position.x) *
                            Mathf.Rad2Deg + 90;

                        float deltaAngle = _dragAngle - dragAngle;
                        Vector3 eulerAngles = playerCamera.transform.rotation.eulerAngles;
                        _deltaAngle = deltaAngle;
                        float targetAngle = eulerAngles.y + deltaAngle;
                        targetAngle = Mathf.MoveTowardsAngle(eulerAngles.y, targetAngle, Time.deltaTime * 360f);

                        playerCamera.transform.rotation =
                            Quaternion.Euler(eulerAngles.x, targetAngle, eulerAngles.z);

                        _forward = playerCamera.transform.forward;
                    }
                }
            }

            if (IsMiddleMouseReleasedThisFrame())
            {
                _isDragging = false;
            }

            if (!_isDragging)
            {
                Vector3 point = GetPoint();
                Vector2 distanceToPlayer = new Vector2(point.x, point.z) -
                                           new Vector2(transform.position.x, transform.position.z);

                distanceToPlayer = Vector2.ClampMagnitude(distanceToPlayer, maxOffset);
                _dampedOffsetVector = Vector2.SmoothDamp(_dampedOffsetVector, distanceToPlayer,
                    ref _currentDistanceVectorVelocity, offsetDampTime);

                _dampedCameraPosition = Vector3.SmoothDamp(_dampedCameraPosition,
                    new Vector3(transform.position.x + cameraPositionOffset.x, transform.position.y, transform.position.z + _currentCameraVertical) - _forward * _currentCameraHeight, ref _cameraVelocity, cameraDampTime);
            }
            else
            {
                _dampedCameraPosition = transform.position - _forward * _currentCameraHeight;
            }

            playerCamera.transform.position = _dampedCameraPosition + new Vector3(_dampedOffsetVector.x, 0, _dampedOffsetVector.y);
        }

        private void CameraInputs()
        {
            HandleCameraZoom();
        }
        
        private void CameraLogic()
        {
            if (!cameraEnabled) return;
            CameraInputs();
            LerpCameraHeight();
        }
        
        private void LerpCameraHeight()
        {
            _currentCameraHeight = Mathf.Lerp(_currentCameraHeight, _cameraHeightTarget, Time.deltaTime * cameraZoomSpeed);
            _currentCameraVertical = Mathf.Lerp(_currentCameraVertical, _cameraVerticalTarget, Time.deltaTime * cameraZoomSpeed);
        }
        
        private void HandleCameraZoom()
        {
            float scrollY = ReadMouseScrollDeltaY();
            if (Mathf.Approximately(scrollY, 0f)) return;

            float heightDifference = scrollY < 0f ? cameraZoomPower : -cameraZoomPower;
            _cameraHeightTarget = _currentCameraHeight + heightDifference;
            _cameraVerticalTarget = _currentCameraVertical + heightDifference;
            if (_cameraHeightTarget > maxCameraHeight) _cameraHeightTarget = maxCameraHeight;
            else if (_cameraHeightTarget < minCameraHeight) _cameraHeightTarget = minCameraHeight;
            if (_cameraVerticalTarget > maxCameraVertical) _cameraVerticalTarget = maxCameraVertical;
            else if (_cameraVerticalTarget < minCameraVertical) _cameraVerticalTarget = minCameraVertical;
        }
        
        private void InitCamera()
        {
            if (!initCameraOnSpawn && playerCamera != null) return;
            Camera cam = GameObject.Find(cameraName).GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError(
                    "TOPDOWN_WASD_CONTROLLER: NO CAMERA FOUND! MAKE SURE TO EITHER DRAG AND DROP ONE, OR ENABLE INIT CAMERA AND TYPE A VALID CAMERA NAME");
            }
            else
            {
                playerCamera = cam;
            }

            if (playerCamera == null) return;
            playerCamera.transform.eulerAngles = cameraRotationOffset;
            _forward = playerCamera.transform.forward;
            InstantCameraUpdate();
        }
        
        private void InitCameraValues()
        {
            _currentCameraHeight = cameraPositionOffset.y;
            _cameraHeightTarget = _currentCameraHeight;
            _currentCameraVertical = cameraPositionOffset.z;
            _cameraVerticalTarget = _currentCameraVertical;
        }
        
        void InstantCameraUpdate()
        {
            Vector3 targetPos = transform.position - (playerCamera.transform.forward * _currentCameraHeight);
            targetPos.z -= _currentCameraVertical;
            _dampedCameraPosition = targetPos;
            playerCamera.transform.position = targetPos;
        }

        private void HandleMovement()
        {
            _displacement.y = 0;
            float deltaTime = Time.deltaTime;
            Vector2 rawInput = GetMovementInput();

            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= deltaTime;
            }

            float cameraYaw = playerCamera != null
                ? playerCamera.transform.rotation.eulerAngles.y
                : transform.rotation.eulerAngles.y;
            Vector2 moveDirection = Rotate(rawInput.normalized, Direction(cameraYaw * Mathf.Deg2Rad));

            if (CanStartDash())
            {
                StartDash(moveDirection);
            }

            Vector2 input = moveDirection * (_actualMoveSpeed * deltaTime);
            if (!_characterController.isGrounded)
            {
                input *= AirControlMultiplier;
            }

            if (_targetPosition.HasValue)
            {
                Vector2 value = _targetPosition.Value;
                Vector2 displacementToTarget = value - new Vector2(transform.position.x, transform.position.z);
                input = Vector2.ClampMagnitude(displacementToTarget, _actualMoveSpeed * deltaTime);
            }

            if (_isDashing)
            {
                float dashDistanceThisFrame = Mathf.Min(_dashSpeed * deltaTime, _dashRemainingDistance);
                _dashRemainingDistance -= dashDistanceThisFrame;
                if (_dashRemainingDistance <= 0f)
                {
                    _isDashing = false;
                }

                input = new Vector2(_dashDirection.x, _dashDirection.z) * dashDistanceThisFrame;
                _targetPosition = null;
            }

            if (_characterController.isGrounded)
            {
                _verticalSpeed = 0f;
                _isJumping = false;

                if (IsKeyPressedThisFrame(jumpKey))
                {
                    _targetPosition = null;
                    _verticalSpeed = jumpHeight;
                    _anim.SetTrigger(JumpHash);
                    _isJumping = true;
                }

                _anim.SetBool(FallingHash, false);
            }
            else
            {
                _targetPosition = null;
                _isJumping = true;

                Vector3 point = GetPoint();
                _lookAngle = Mathf.Atan2(transform.position.z - point.z, point.x - transform.position.x) *
                    Mathf.Rad2Deg + 90;

                if (GetGroundDistance() > 0.2f) _anim.SetBool(FallingHash, true);
                else _anim.SetBool(FallingHash, false);
            }

            if (input != Vector2.zero)
            {
                _forwardF = Mathf.Atan2(-input.y, input.x) * Mathf.Rad2Deg + 90;

                _displacement.x = input.x;
                _displacement.z = input.y;

                _anim.SetLayerWeight(attackAnimatorLayer, 1);
                _anim.SetLayerWeight(2, 0);
            }
            else
            {
                _displacement.x = 0.0f;
                _displacement.z = 0.0f;

                _anim.SetLayerWeight(attackAnimatorLayer, ShouldKeepAttackLayerEnabled() ? 1 : 0);
                _anim.SetLayerWeight(2, 1);
            }

            if (_characterController.isGrounded)
            {
                _lookAngle =
                    Mathf.Atan2(transform.position.z - GetPoint().z, GetPoint().x - transform.position.x) *
                    Mathf.Rad2Deg + 90;
            }

            if (input != Vector2.zero)
            {
                float deltaAngle = Mathf.DeltaAngle(_lookAngle, _forwardF);
                float differenceAngle = Mathf.Round(deltaAngle / 45) * 45;

                _targetRotationAngle = _forwardF - differenceAngle;

                float horizontal = Mathf.Round(Mathf.Sin(differenceAngle * Mathf.Deg2Rad));
                float vertical = Mathf.Round(Mathf.Cos(differenceAngle * Mathf.Deg2Rad));

                _anim.SetFloat(HorizontalHash, horizontal, animatorSmoothTime, Time.deltaTime);
                _anim.SetFloat(VerticalHash, vertical, animatorSmoothTime, Time.deltaTime);
            }
            else
            {
                float deltaAngle = Mathf.DeltaAngle(_forwardF, _lookAngle);

                if (deltaAngle < -90)
                {
                    _targetRotationAngle = _forwardF - 90f;
                    _forwardF = _forwardF - 90f;
                }

                if (deltaAngle > 90)
                {
                    _targetRotationAngle = _forwardF + 90f;
                    _forwardF = _forwardF + 90f;
                }

                _anim.SetFloat(HorizontalHash, 0, animatorSmoothTime, Time.deltaTime);
                _anim.SetFloat(VerticalHash, 0, animatorSmoothTime, Time.deltaTime);
            }

            if (!Mathf.Approximately(_rotationAngle, _targetRotationAngle))
            {
                _rotationAngle = Mathf.SmoothDampAngle(_rotationAngle, _targetRotationAngle,
                    ref _angularVelocity,
                    dampSmoothTimeRotation);

                transform.rotation = Quaternion.Euler(0, _rotationAngle, 0);
            }

            if (_characterController.isGrounded && !_isJumping)
            {
                _displacement.y = -gravity * deltaTime;
            }
            else
            {
                _displacement.y = _verticalSpeed * deltaTime;
            }

            _verticalSpeed -= gravity * deltaTime;

            _characterController.Move(_displacement);
        }

        private void HandleAttack()
        {
            if (_attackWarmupFrames > 0)
            {
                _attackWarmupFrames--;
            }

            if (IsLeftMousePressedThisFrame())
            {
                if (_attackComboStep <= 0 || _attackComboTimer <= 0f)
                {
                    PlayAttackStep(1);
                }
                else
                {
                    int remainingFollowUps = MaxAttackComboStep - _attackComboStep;
                    if (remainingFollowUps > 0)
                    {
                        _bufferedAttackInputs = Mathf.Min(_bufferedAttackInputs + 1, remainingFollowUps);
                    }
                }
            }

            if (TryAdvanceBufferedAttack())
            {
                return;
            }

            UpdateComboResetTimer();
        }

        private bool TryAdvanceBufferedAttack()
        {
            if (_anim == null || _bufferedAttackInputs <= 0 || _attackComboStep <= 0)
            {
                return false;
            }

            if (!CanAdvanceAttackCombo())
            {
                return false;
            }

            int nextStep = _attackComboStep >= MaxAttackComboStep ? 1 : _attackComboStep + 1;
            if (nextStep == _attackComboStep)
            {
                return false;
            }

            _bufferedAttackInputs--;
            PlayAttackStep(nextStep);
            return true;
        }

        private bool CanAdvanceAttackCombo()
        {
            if (_anim == null)
            {
                return true;
            }

            AnimatorStateInfo currentState = _anim.GetCurrentAnimatorStateInfo(attackAnimatorLayer);
            if (!IsAttackState(currentState.shortNameHash))
            {
                return true;
            }

            if (_anim.IsInTransition(attackAnimatorLayer))
            {
                return false;
            }

            return currentState.normalizedTime >= attackReleaseNormalizedTime;
        }

        private bool IsAttackAnimationPlaying()
        {
            if (_anim == null)
            {
                return false;
            }

            AnimatorStateInfo currentState = _anim.GetCurrentAnimatorStateInfo(attackAnimatorLayer);
            if (IsAttackState(currentState.shortNameHash))
            {
                return currentState.normalizedTime < attackReleaseNormalizedTime;
            }

            if (!_anim.IsInTransition(attackAnimatorLayer))
            {
                return false;
            }

            AnimatorStateInfo nextState = _anim.GetNextAnimatorStateInfo(attackAnimatorLayer);
            return IsAttackState(nextState.shortNameHash);
        }

        private bool ShouldKeepAttackLayerEnabled()
        {
            return _attackComboStep > 0 || _bufferedAttackInputs > 0 || _attackWarmupFrames > 0 || IsAttackAnimationPlaying();
        }

        private void PlayAttackStep(int step)
        {
            _attackComboStep = step;
            _attackComboTimer = attackComboResetTime;
            _attackWarmupFrames = 2;

            if (_anim == null)
            {
                return;
            }

            _anim.SetLayerWeight(attackAnimatorLayer, 1);

            switch (step)
            {
                case 1:
                    _anim.Play(Attack1Hash, attackAnimatorLayer, 0f);
                    break;
                case 2:
                    _anim.Play(Attack2Hash, attackAnimatorLayer, 0f);
                    break;
                case 3:
                    _anim.Play(Attack3Hash, attackAnimatorLayer, 0f);
                    break;
            }
        }

        private void UpdateComboResetTimer()
        {
            if (_attackComboStep <= 0 || _bufferedAttackInputs > 0 || _attackWarmupFrames > 0)
            {
                return;
            }

            if (IsAttackAnimationPlaying())
            {
                _attackComboTimer = attackComboResetTime;
                return;
            }

            if (_attackComboTimer <= 0f)
            {
                _attackComboTimer = attackComboResetTime;
            }

            _attackComboTimer -= Time.deltaTime;
            if (_attackComboTimer <= 0f)
            {
                ResetComboState();
            }
        }

        private void ResetComboState()
        {
            _attackComboStep = 0;
            _attackComboTimer = 0f;
            _bufferedAttackInputs = 0;
            _attackWarmupFrames = 0;
        }

        private static bool IsAttackState(int shortNameHash)
        {
            return shortNameHash == Attack1Hash
                   || shortNameHash == Attack2Hash
                   || shortNameHash == Attack3Hash;
        }

        private bool CanStartDash()
        {
            return !_isDashing
                   && _dashCooldownTimer <= 0f
                   && dashDistance > 0f
                   && dashDuration > 0f
                   && IsKeyPressedThisFrame(dashKey);
        }


        private void StartDash(Vector2 moveDirection)
        {
            Vector3 dashDirection = moveDirection.sqrMagnitude > 0.0001f
                ? new Vector3(moveDirection.x, 0f, moveDirection.y).normalized
                : transform.forward;

            dashDirection.y = 0f;
            if (dashDirection.sqrMagnitude < 0.0001f)
            {
                dashDirection = Vector3.forward;
            }

            _dashDirection = dashDirection.normalized;
            _dashSpeed = (dashDistance / dashDuration) * dashSpeedMultiplier;
            _dashRemainingDistance = dashDistance;
            _dashCooldownTimer = dashCooldown;
            _isDashing = true;
            if (_anim != null)
            {
                _anim.SetTrigger(DashHash);
            }
            _forwardF = Mathf.Atan2(-_dashDirection.z, _dashDirection.x) * Mathf.Rad2Deg + 90;
        }

        private float GetGroundDistance()
        {
            if (Physics.Raycast (transform.position, -Vector3.up, out var hit)) {
                return hit.distance;
            }
            return 0;
        }

        private Vector2 GetMovementInput()
        {
            Vector2 v2Input = new Vector2();
            if (movementInputType == MovementInputType.Keyboard)
            {
                if (IsKeyPressed(moveUpKey))
                {
                    v2Input.y = 1;
                }

                if (IsKeyPressed(moveDownKey))
                {
                    v2Input.y -= 1;
                }

                if (IsKeyPressed(moveLeftKey))
                {
                    v2Input.x -= 1;
                }

                if (IsKeyPressed(moveRightKey))
                {
                    v2Input.x = 1;
                }

                _targetPosition = null;
            }

            return v2Input;
        }

        static Vector2 Rotate(Vector2 self, Vector2 other)
        {
            return new Vector2(self.x * other.x - self.y * other.y, self.x * other.y + other.x * self.y);
        }

        static Vector2 Direction(float angle)
        {
            return new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle));
        }

        private Vector3 GetPoint()
        {
            if (playerCamera == null)
            {
                return transform.position;
            }

            var playerPlane = new Plane(Vector3.up, transform.position);
            Vector2 mousePosition = ReadMousePosition();
            var ray = playerCamera.ScreenPointToRay(mousePosition);
            return playerPlane.Raycast(ray, out var hitDist) ? ray.GetPoint(hitDist) : Vector3.zero;
        }

        private static bool IsKeyPressed(KeyCode keyCode)
        {
            return Keyboard.current != null && TryGetKey(keyCode, out Key key) && Keyboard.current[key].isPressed;
        }

        private static bool IsKeyPressedThisFrame(KeyCode keyCode)
        {
            return Keyboard.current != null && TryGetKey(keyCode, out Key key) && Keyboard.current[key].wasPressedThisFrame;
        }

        private static bool IsMiddleMousePressedThisFrame()
        {
            return Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame;
        }

        private static bool IsMiddleMousePressed()
        {
            return Mouse.current != null && Mouse.current.middleButton.isPressed;
        }

        private static bool IsMiddleMouseReleasedThisFrame()
        {
            return Mouse.current != null && Mouse.current.middleButton.wasReleasedThisFrame;
        }

        private static bool IsLeftMousePressedThisFrame()
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        private static Vector2 ReadMousePosition()
        {
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        }

        private static float ReadMouseScrollDeltaY()
        {
            return Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        }

        private static bool TryGetKey(KeyCode keyCode, out Key key)
        {
            switch (keyCode)
            {
                case KeyCode.W:
                    key = Key.W;
                    return true;
                case KeyCode.A:
                    key = Key.A;
                    return true;
                case KeyCode.S:
                    key = Key.S;
                    return true;
                case KeyCode.D:
                    key = Key.D;
                    return true;
                case KeyCode.Space:
                    key = Key.Space;
                    return true;
                case KeyCode.LeftArrow:
                    key = Key.LeftArrow;
                    return true;
                case KeyCode.RightArrow:
                    key = Key.RightArrow;
                    return true;
                case KeyCode.UpArrow:
                    key = Key.UpArrow;
                    return true;
                case KeyCode.DownArrow:
                    key = Key.DownArrow;
                    return true;
                case KeyCode.LeftShift:
                    key = Key.LeftShift;
                    return true;
                case KeyCode.RightShift:
                    key = Key.RightShift;
                    return true;
                default:
                    return Enum.TryParse(keyCode.ToString(), true, out key);
            }
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (_anim == null) return;

            _anim.SetLookAtWeight(1, bodyWeightIK, headWeightIK, 1.0f);

            float targetDeltaAngle = Mathf.Clamp(Mathf.DeltaAngle(_rotationAngle, _lookAngle), -60, 60);

            _deltaAngle =
                Mathf.SmoothDampAngle(_deltaAngle, targetDeltaAngle, ref _deltaAngleVelocity, dampSmoothTimeIK);

            _targetAngle = (_rotationAngle + _deltaAngle - 90) * Mathf.Deg2Rad;

            Vector3 targetLookAt = transform.position +
                                   new Vector3(Mathf.Cos(_targetAngle) * 10, 0, Mathf.Sin(_targetAngle) * -10);

            Transform headTransform = _anim.GetBoneTransform(HumanBodyBones.Head);

            targetLookAt.y = headTransform.position.y;

            _anim.SetLookAtPosition(targetLookAt);
        }
    }
}
