using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using ThePromisedRun.Gameplay;
using ThePromisedRun.Gameplay.Input;
using ThePromisedRun.Gameplay.Player.ScriptableObjects;

namespace ThePromisedRun.Tests {
    public class PlayerControllerMovementTests {
        private GameObject       _root;
        private PlayerController _controller;
        private Transform        _visual;
        private Rigidbody        _rb;
        private InputReader      _inputReader;
        private PlayerProperties _props;

        [SetUp]
        public void SetUp() {
            // Disable root so AddComponent<PlayerController> does NOT auto-trigger Awake().
            // This lets us inject _playerProperties before Awake() runs.
            _root = new GameObject("Player");
            _root.SetActive(false);

            // Add Rigidbody (required by ApplyMovement)
            _rb = _root.AddComponent<Rigidbody>();

            // Add InputReader
            _inputReader = _root.AddComponent<InputReader>();

            // Create Visual child parented to root (needs Animator so PlayerController.Awake() doesn't NPE)
            GameObject visualGO = new GameObject("Visual");
            visualGO.transform.SetParent(_root.transform);
            visualGO.AddComponent<Animator>();
            _visual = visualGO.transform;

            // Add PlayerController while root is inactive — Awake() will NOT fire yet
            _controller = _root.AddComponent<PlayerController>();

            // Inject PlayerProperties BEFORE invoking Awake() so LoadPlayerProperties() doesn't NPE.
            _props = ScriptableObject.CreateInstance<PlayerProperties>();
            typeof(PlayerController)
                .GetField("_playerProperties", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_controller, _props);

            // Now manually invoke Awake() — _playerProperties is set so it succeeds
            typeof(PlayerController)
                .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_controller, null);

            // Override _moveSpeed to the expected test value (8f)
            typeof(PlayerController)
                .GetField("_moveSpeed", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_controller, 8f);
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_props);
        }

        private void SetMoveInput(float x, float y) {
            typeof(InputReader)
                .GetProperty("MoveInput")
                .SetValue(_inputReader, new Vector2(x, y));
        }

        // Task 1.2 — Confirms bug: root scale is flipped to -1 when moving left
        [Test]
        public void ApplyMovement_MoveLeft_SetsNegativeScaleOnRoot() {
            SetMoveInput(-1f, 0f);
            _controller.ApplyMovement();
            Assert.AreEqual(-1f, _root.transform.localScale.x);
        }

        // Task 1.3 — Confirms fix not yet applied: visual is NOT rotated when moving left
        [Test]
        public void ApplyMovement_MoveLeft_DoesNotRotateVisual() {
            SetMoveInput(-1f, 0f);
            _controller.ApplyMovement();
            Assert.That(
                Quaternion.Angle(_visual.localRotation, Quaternion.identity),
                Is.LessThan(0.01f)
            );
        }

        // Task 1.4 — Confirms bug persists after stopping: scale stays -1 when player stops
        [Test]
        public void ApplyMovement_StopAfterLeft_RootScaleRemainsNegative() {
            SetMoveInput(-1f, 0f);
            _controller.ApplyMovement();
            SetMoveInput(0f, 0f);
            _controller.ApplyMovement();
            Assert.AreEqual(-1f, _root.transform.localScale.x);
        }

        // ===== Fix-Checking Tests (Property 1) =====

        // Task 3.1 — Fix check: moving left rotates visual to face left (Y=180)
        [Test]
        public void ApplyMovement_MoveLeft_RotatesVisualToFaceLeft() {
            SetMoveInput(-1f, 0f);
            _controller.ApplyMovement();
            Assert.That(Quaternion.Angle(_visual.localRotation, Quaternion.Euler(0, 180, 0)), Is.LessThan(0.01f));
        }

        // Task 3.2 — Fix check: moving right rotates visual to face right (identity)
        [Test]
        public void ApplyMovement_MoveRight_RotatesVisualToFaceRight() {
            SetMoveInput(1f, 0f);
            _controller.ApplyMovement();
            Assert.That(Quaternion.Angle(_visual.localRotation, Quaternion.identity), Is.LessThan(0.01f));
        }

        // Task 3.3 — Fix check: moving left does NOT flip root scale
        [Test]
        public void ApplyMovement_MoveLeft_RootScaleUnchanged() {
            SetMoveInput(-1f, 0f);
            _controller.ApplyMovement();
            Assert.AreEqual(Vector3.one, _root.transform.localScale);
        }

        // Task 3.4 — Fix check: moving right does NOT flip root scale
        [Test]
        public void ApplyMovement_MoveRight_RootScaleUnchanged() {
            SetMoveInput(1f, 0f);
            _controller.ApplyMovement();
            Assert.AreEqual(Vector3.one, _root.transform.localScale);
        }

        // Task 3.5 — Fix check: direction switch updates visual rotation immediately (same frame)
        [Test]
        public void ApplyMovement_DirectionSwitch_VisualRotationUpdatesImmediately() {
            SetMoveInput(1f, 0f);
            _controller.ApplyMovement();
            SetMoveInput(-1f, 0f);
            _controller.ApplyMovement();
            Assert.That(Quaternion.Angle(_visual.localRotation, Quaternion.Euler(0, 180, 0)), Is.LessThan(0.01f));
        }

        // ===== Preservation-Checking Tests (Property 2) =====

        // Task 4.1 — Preservation: zero X input does not change visual rotation
        [Test]
        public void ApplyMovement_ZeroX_DoesNotChangeVisualRotation() {
            _visual.localRotation = Quaternion.Euler(0, 45, 0);
            SetMoveInput(0f, 1f);
            _controller.ApplyMovement();
            Assert.That(Quaternion.Angle(_visual.localRotation, Quaternion.Euler(0, 45, 0)), Is.LessThan(0.01f));
        }

        // Task 4.2 — Preservation: Z-axis movement applies correct velocity on Z and zero on X
        [Test]
        public void ApplyMovement_ZAxisMovement_AppliesCorrectVelocityZ() {
            SetMoveInput(0f, 1f);
            _controller.ApplyMovement();
            Assert.AreEqual(8f, _rb.linearVelocity.z, 0.001f);
            Assert.AreEqual(0f, _rb.linearVelocity.x, 0.001f);
        }

        // Task 4.3 — Preservation: idle input results in zero X velocity
        [Test]
        public void ApplyMovement_Idle_VelocityXIsZero() {
            SetMoveInput(0f, 0f);
            _controller.ApplyMovement();
            Assert.AreEqual(0f, _rb.linearVelocity.x, 0.001f);
        }

        // Task 4.4 — Preservation: null visual does not throw when applying movement
        [Test]
        public void ApplyMovement_NullVisual_DoesNotThrow() {
            typeof(PlayerController)
                .GetField("_visual", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_controller, null);
            SetMoveInput(-1f, 0f);
            Assert.DoesNotThrow(() => _controller.ApplyMovement());
        }

        // Task 4.5 — Preservation (property-based): zero X input never changes root scale
        [Test]
        public void ApplyMovement_AnyZeroXInput_RootScaleAlwaysOne() {
            float[] yValues = { -1f, -0.75f, -0.5f, -0.25f, 0f, 0.25f, 0.5f, 0.75f, 1f, 0.1f, 0.9f, -0.1f, -0.9f, 0.33f, -0.33f, 0.66f, -0.66f, 0.01f, -0.01f, 0.99f };
            foreach (float yValue in yValues) {
                SetMoveInput(0f, yValue);
                _controller.ApplyMovement();
                Assert.AreEqual(Vector3.one, _root.transform.localScale, $"Scale was not (1,1,1) for MoveInput.y={yValue}");
            }
        }
    }
}
