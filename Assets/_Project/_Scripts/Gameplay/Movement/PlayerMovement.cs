using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace ThePromisedRun.Gameplay.Movement {
    /// <summary>
    /// Player movement component - separated for Single Responsibility Principle
    /// Handles all movement physics and ground detection
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        [SerializeField] private float airControl = 0.5f;
        
        [Header("References")]
        [SerializeField] private Transform visual;
        [SerializeField] private Component groundDetector;
        
        private Rigidbody _rb;
        private bool _isGrounded;
        private Vector2 _moveInput;
        private bool _isJumpPressed;
        
        public bool IsGrounded => _isGrounded;
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float JumpForce { get => jumpForce; set => jumpForce = value; }
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
        public bool IsJumpPressed => _isJumpPressed;
        
        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            if (visual == null) {
                visual = FindChildByName("Visual");
            }
            if (groundDetector == null) {
                var detector = FindChildByName("Detector");
                if (detector != null) {
                    groundDetector = detector.GetComponent<Component>();
                }
            }
        }
        
        private Transform FindChildByName(string name) {
            return Enumerable.Range(0, transform.childCount)
                .Select(i => transform.GetChild(i))
                .FirstOrDefault(c => c.name == name);
        }
        
        public void SetInput(Vector2 input) {
            _moveInput = input;
        }
        
        public void SetJumpInput(bool pressed) {
            _isJumpPressed = pressed;
        }
        
        private void Update() {
            CheckGround();
            ApplyMovement();
            
            if (_isJumpPressed && _isGrounded) {
                ApplyJump();
                _isJumpPressed = false;
            }
        }
        
        private void FixedUpdate() {
            ApplyExtraGravity();
        }
        
        private void CheckGround() {
            if (groundDetector == null) {
                _isGrounded = false;
                return;
            }
            
            var detector = groundDetector as MonoBehaviour;
            if (detector != null) {
                try {
                    var performed = detector.GetType().GetProperty("Performed");
                    if (performed != null) {
                        _isGrounded = (bool)performed.GetValue(detector);
                        return;
                    }
                }
                catch { }
            }
            _isGrounded = false;
        }
        
        private void ApplyMovement() {
            Vector3 moveDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
            if (moveDir.sqrMagnitude < 0.01f) return;
            
            float actualSpeed = _isGrounded ? moveSpeed : moveSpeed * airControl;
            
            _rb.linearVelocity = new Vector3(
                moveDir.x * actualSpeed,
                _rb.linearVelocity.y,
                moveDir.z * actualSpeed
            );
            
            Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRot, 
                rotationSpeed * Time.deltaTime
            );
        }
        
        private void ApplyJump() {
            _rb.linearVelocity = new Vector3(
                _rb.linearVelocity.x,
                jumpForce,
                _rb.linearVelocity.z
            );
        }
        
        private void ApplyExtraGravity() {
            if (_rb.linearVelocity.y < 0f) {
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }
        
        public void StopMovement() {
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        }
    }
}