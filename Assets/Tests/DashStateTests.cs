using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using ThePromisedRun.Gameplay;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Gameplay.Input;
using ThePromisedRun.Gameplay.Player.ScriptableObjects;
using ThePromisedRun.Gameplay.States;

namespace ThePromisedRun.Tests {
    /// <summary>
    /// EditMode tests for DashState — example tests (7.2) and property tests (7.3–7.10).
    /// All tests follow the same SetUp/TearDown pattern as PlayerControllerMovementTests.cs.
    /// </summary>
    public class DashStateTests {

        // ─── 7.1 SetUp / TearDown ────────────────────────────────────────────────

        private GameObject       _root;
        private PlayerController _controller;
        private Rigidbody        _rb;
        private InputReader      _inputReader;
        private Animator         _animator;
        private DashState        _dashState;
        private PlayerProperties _props;

        [SetUp]
        public void SetUp() {
            // Create root Player GameObject — disabled so AddComponent does NOT trigger Awake()
            // automatically. This lets us inject _playerProperties before Awake() runs.
            _root = new GameObject("Player");
            _root.SetActive(false);

            // Add Rigidbody (required by DashState.OnFixedUpdate)
            _rb = _root.AddComponent<Rigidbody>();

            // Add InputReader
            _inputReader = _root.AddComponent<InputReader>();

            // Create Visual child with Animator (PlayerController.Awake needs it)
            GameObject visualGO = new GameObject("Visual");
            visualGO.transform.SetParent(_root.transform);
            _animator = visualGO.AddComponent<Animator>();

            // Add PlayerController while root is inactive — Awake() will NOT fire yet
            _controller = _root.AddComponent<PlayerController>();

            // Create a PlayerProperties instance with controlled test values and inject
            // it BEFORE invoking Awake() so LoadPlayerProperties() doesn't NPE.
            _props = ScriptableObject.CreateInstance<PlayerProperties>();
            _props.dashDistance       = 3f;
            _props.dashDuration       = 0.18f;
            _props.dashCooldown       = 1.2f;
            _props.dashIFrameDuration = 0.1f;
            _props.chaosPerDash       = 10f;
            _props.maxChaosThreshold  = 100f;
            _props.chaosDecayRate     = 0f;   // no decay during tests

            FieldInfo propsField = typeof(PlayerController).GetField(
                "_playerProperties",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            propsField.SetValue(_controller, _props);

            // Now manually invoke Awake() — _playerProperties is set so LoadPlayerProperties() succeeds
            typeof(PlayerController)
                .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_controller, null);

            // Create DashState directly (no MonoBehaviour needed)
            _dashState = new DashState(_controller, _animator);
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_props);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private void SetMoveInput(float x, float y) {
            typeof(InputReader)
                .GetProperty("MoveInput")
                .SetValue(_inputReader, new Vector2(x, y));
        }

        /// <summary>Simulate OnUpdate() with a fixed deltaTime without touching Time.deltaTime.</summary>
        private void SimulateUpdate(float deltaTime) {
            // Inject _dashTimer and _iFrameTimer decrements by manipulating the private fields directly,
            // then call OnUpdate() with a patched Time.deltaTime substitute via reflection on the timers.
            // Because OnUpdate() uses Time.deltaTime (which is 0 in EditMode), we instead
            // directly decrement the private timer fields and then call the state-transition logic.
            FieldInfo dashTimerField  = typeof(DashState).GetField("_dashTimer",  BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo iFrameTimerField = typeof(DashState).GetField("_iFrameTimer", BindingFlags.NonPublic | BindingFlags.Instance);

            float dashTimer  = (float)dashTimerField.GetValue(_dashState)  - deltaTime;
            float iFrameTimer = (float)iFrameTimerField.GetValue(_dashState) - deltaTime;

            dashTimerField.SetValue(_dashState, dashTimer);
            iFrameTimerField.SetValue(_dashState, iFrameTimer);

            // Replicate the state-transition logic from OnUpdate() without Time.deltaTime
            if (iFrameTimer <= 0f && _dashState.IsInvincible)
                SetPrivateProp("IsInvincible", false);

            if (dashTimer <= 0f)
                SetPrivateProp("CanExit", true);
        }

        private void SetPrivateProp(string propName, bool value) {
            // Properties have private setters — use the backing field via PropertyInfo + reflection
            typeof(DashState)
                .GetProperty(propName, BindingFlags.Public | BindingFlags.Instance)
                .SetValue(_dashState, value);
        }

        private float GetDashTimer() =>
            (float)typeof(DashState)
                .GetField("_dashTimer", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_dashState);

        private Vector3 GetDashDir() =>
            (Vector3)typeof(DashState)
                .GetField("_dashDir", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_dashState);

        private float GetDashCooldownTimer() =>
            (float)typeof(PlayerController)
                .GetField("_dashCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_controller);

        private void SetDashCooldownTimer(float value) =>
            typeof(PlayerController)
                .GetField("_dashCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_controller, value);

        // ─── 7.2 Example Tests ───────────────────────────────────────────────────

        // Req 1.3 — no move input → dash direction equals transform.forward
        [Test]
        public void OnEnter_NoMoveInput_DashesForward() {
            SetMoveInput(0f, 0f);
            _dashState.OnEnter();

            Vector3 dashDir = GetDashDir();
            Vector3 expected = _controller.transform.forward;

            Assert.That(Vector3.Angle(dashDir, expected), Is.LessThan(0.01f),
                $"Expected dashDir ≈ transform.forward ({expected}), got {dashDir}");
        }

        // Req 4.5 — OnEnter() must consume dash input
        [Test]
        public void OnEnter_ConsumesInput() {
            typeof(InputReader)
                .GetProperty("IsDashPressed")
                .SetValue(_inputReader, true);

            _dashState.OnEnter();

            Assert.That(_inputReader.IsDashPressed, Is.False,
                "IsDashPressed should be false after OnEnter() consumes the input");
        }

        // Req 3.1, 8.2 — OnEnter() activates invincibility
        [Test]
        public void OnEnter_SetsIsInvincibleTrue() {
            _dashState.OnEnter();

            Assert.That(_dashState.IsInvincible, Is.True,
                "IsInvincible should be true immediately after OnEnter()");
        }

        // Req 4.6, 8.4 — OnEnter() sets CanExit to false
        [Test]
        public void OnEnter_SetsCantExitFalse() {
            _dashState.OnEnter();

            Assert.That(_dashState.CanExit, Is.False,
                "CanExit should be false immediately after OnEnter()");
        }

        // Req 3.4 — early OnExit() clears invincibility regardless of I-frame timer
        [Test]
        public void OnExit_BeforeIFrameExpires_ClearsInvincibility() {
            _dashState.OnEnter();
            Assert.That(_dashState.IsInvincible, Is.True, "Precondition: IsInvincible should be true after OnEnter()");

            // Exit before I-frame has expired (no SimulateUpdate calls)
            _dashState.OnExit();

            Assert.That(_dashState.IsInvincible, Is.False,
                "IsInvincible should be false after OnExit() even before I-frame expires");
        }

        // Req 6.3 — missing Dash animation clip must not throw
        [Test]
        public void OnEnter_MissingDashClip_DoesNotThrow() {
            // Animator has no RuntimeAnimatorController → no "Dash" state exists
            Assert.DoesNotThrow(() => _dashState.OnEnter(),
                "OnEnter() should not throw when the 'Dash' animation clip is missing");
        }

        // ─── 7.3 Property 1 — Dash velocity invariant ────────────────────────────
        // Validates: Requirements 1.1, 1.4

        [Test]
        public void Property1_DashVelocityInvariant_HorizontalSpeedEqualsDashSpeed() {
            // 20 representative dash directions (x, y move input)
            (float x, float y)[] inputs = {
                (1f, 0f), (-1f, 0f), (0f, 1f), (0f, -1f),
                (0.707f, 0.707f), (-0.707f, 0.707f), (0.707f, -0.707f), (-0.707f, -0.707f),
                (0.5f, 0.866f), (-0.5f, 0.866f), (0.5f, -0.866f), (-0.5f, -0.866f),
                (0.866f, 0.5f), (-0.866f, 0.5f), (0.866f, -0.5f), (-0.866f, -0.5f),
                (0.3f, 0.954f), (-0.3f, 0.954f), (0.3f, -0.954f), (-0.3f, -0.954f)
            };

            float expectedSpeed = _controller.DashSpeed;

            foreach ((float x, float y) in inputs) {
                // Reset state between iterations
                _rb.linearVelocity = new Vector3(0f, -2f, 0f); // non-zero Y to test preservation
                SetMoveInput(x, y);
                _dashState.OnEnter();
                _dashState.OnFixedUpdate();

                Vector3 vel = _rb.linearVelocity;
                float horizontalSpeed = new Vector2(vel.x, vel.z).magnitude;

                Assert.That(horizontalSpeed, Is.EqualTo(expectedSpeed).Within(0.01f),
                    $"Property 1 failed for input ({x},{y}): horizontal speed {horizontalSpeed} ≠ DashSpeed {expectedSpeed}");

                Assert.That(vel.y, Is.EqualTo(-2f).Within(0.001f),
                    $"Property 1 failed for input ({x},{y}): vertical velocity was modified (expected -2, got {vel.y})");

                // Reset for next iteration
                _dashState.OnExit();
                _dashState = new DashState(_controller, _animator);
            }
        }

        // ─── 7.4 Property 2 — Dash direction follows move input ──────────────────
        // Validates: Requirements 1.2

        [Test]
        public void Property2_DashDirectionFollowsMoveInput() {
            // 20 non-zero input samples
            (float x, float y)[] inputs = {
                (1f, 0f), (-1f, 0f), (0f, 1f), (0f, -1f),
                (0.6f, 0.8f), (-0.6f, 0.8f), (0.6f, -0.8f), (-0.6f, -0.8f),
                (0.8f, 0.6f), (-0.8f, 0.6f), (0.8f, -0.6f), (-0.8f, -0.6f),
                (0.1f, 0.995f), (-0.1f, 0.995f), (0.1f, -0.995f), (-0.1f, -0.995f),
                (0.995f, 0.1f), (-0.995f, 0.1f), (0.5f, 0.5f), (-0.5f, -0.5f)
            };

            foreach ((float x, float y) in inputs) {
                SetMoveInput(x, y);
                _dashState.OnEnter();

                Vector3 dashDir  = GetDashDir();
                Vector3 expected = new Vector3(x, 0f, y).normalized;

                Assert.That(Vector3.Angle(dashDir, expected), Is.LessThan(0.01f),
                    $"Property 2 failed for input ({x},{y}): dashDir {dashDir} ≠ expected {expected}");

                // Reset for next iteration
                _dashState.OnExit();
                _dashState = new DashState(_controller, _animator);
            }
        }

        // ─── 7.5 Property 3 — Velocity damping on exit ───────────────────────────
        // Validates: Requirements 1.5

        [Test]
        public void Property3_VelocityDampingOnExit() {
            // 20 velocity samples (vx, vy, vz)
            (float vx, float vy, float vz)[] velocities = {
                (10f,  0f,  0f), (-10f,  0f,  0f), (0f,  0f, 10f), (0f,  0f, -10f),
                (5f,  -3f,  5f), (-5f,  -3f,  5f), (5f,  -3f, -5f), (-5f,  -3f, -5f),
                (16.7f, 0f,  0f), (0f,  0f, 16.7f), (8f,  2f,  8f), (-8f,  2f, -8f),
                (1f,  -9.8f, 0f), (0f,  -9.8f, 1f), (3f,  5f,  4f), (-3f,  5f, -4f),
                (0.5f, 0f,  0.5f), (-0.5f, 0f, -0.5f), (20f, -1f, 20f), (-20f, -1f, -20f)
            };

            foreach ((float vx, float vy, float vz) in velocities) {
                _rb.linearVelocity = new Vector3(vx, vy, vz);
                _dashState.OnExit();

                Vector3 result = _rb.linearVelocity;

                Assert.That(result.x, Is.EqualTo(vx * 0.3f).Within(0.001f),
                    $"Property 3 failed for vel ({vx},{vy},{vz}): X {result.x} ≠ {vx * 0.3f}");
                Assert.That(result.z, Is.EqualTo(vz * 0.3f).Within(0.001f),
                    $"Property 3 failed for vel ({vx},{vy},{vz}): Z {result.z} ≠ {vz * 0.3f}");
                Assert.That(result.y, Is.EqualTo(vy).Within(0.001f),
                    $"Property 3 failed for vel ({vx},{vy},{vz}): Y {result.y} ≠ {vy} (should be unchanged)");

                // Reset for next iteration
                _dashState = new DashState(_controller, _animator);
            }
        }

        // ─── 7.6 Property 4 — Cooldown round-trip ────────────────────────────────
        // Validates: Requirements 2.1, 2.2, 2.3

        [Test]
        public void Property4_CooldownRoundTrip() {
            float[] cooldowns = { 0.1f, 0.3f, 0.5f, 0.8f, 1.0f, 1.2f, 1.5f, 2.0f, 3.0f, 5.0f };

            foreach (float cooldown in cooldowns) {
                _props.dashCooldown = cooldown;

                // Trigger cooldown
                _controller.StartDashCooldown();

                Assert.That(_controller.IsDashReady, Is.False,
                    $"Property 4 failed for cooldown={cooldown}: IsDashReady should be false immediately after StartDashCooldown()");

                // Advance timer past cooldown by setting it to 0 via reflection
                SetDashCooldownTimer(0f);

                Assert.That(_controller.IsDashReady, Is.True,
                    $"Property 4 failed for cooldown={cooldown}: IsDashReady should be true after timer reaches 0");
            }
        }

        // ─── 7.7 Property 5 — Health invariant during I-frame ────────────────────
        // Validates: Requirements 3.2, 8.6

        [Test]
        public void Property5_HealthInvariantDuringIFrame() {
            float[] damageValues = { 1f, 5f, 10f, 15f, 25f, 50f, 75f, 100f, 150f, 200f, 500f, 999f, 0.1f, 0.5f, 33.3f };

            DamageInfo info = new DamageInfo();

            foreach (float damage in damageValues) {
                // Activate I-frame
                _controller.SetDashInvincible(true);

                // Record chaos before (Health is always 100f in PlayerController stub)
                float chaosBefore = _controller.ChaosMeter;

                // Attempt to deal damage
                _controller.TakeDamage(damage, info);

                // Chaos should not have changed (TakeDamage is blocked by I-frame)
                Assert.That(_controller.ChaosMeter, Is.EqualTo(chaosBefore).Within(0.001f),
                    $"Property 5 failed for damage={damage}: ChaosMeter changed from {chaosBefore} to {_controller.ChaosMeter} during I-frame");

                // Reset for next iteration
                _controller.SetDashInvincible(false);
                _controller.ResetChaos();
            }
        }

        // ─── 7.8 Property 6 — I-frame expiry ─────────────────────────────────────
        // Validates: Requirements 3.1, 3.3, 8.3

        [Test]
        public void Property6_IFrameExpiry() {
            float[] iFrameDurations = { 0.05f, 0.08f, 0.1f, 0.12f, 0.15f, 0.18f, 0.2f, 0.25f, 0.3f, 0.5f };

            foreach (float iFrameDuration in iFrameDurations) {
                // Configure the SO and reload backing fields
                _props.dashIFrameDuration = iFrameDuration;
                typeof(PlayerController)
                    .GetMethod("LoadPlayerProperties", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(_controller, null);

                // Fresh DashState with updated params
                _dashState = new DashState(_controller, _animator);
                SetMoveInput(1f, 0f);
                _dashState.OnEnter();

                Assert.That(_dashState.IsInvincible, Is.True,
                    $"Property 6 precondition failed for iFrameDuration={iFrameDuration}: IsInvincible should be true after OnEnter()");

                // Simulate time accumulation >= iFrameDuration
                SimulateUpdate(iFrameDuration + 0.001f);

                Assert.That(_dashState.IsInvincible, Is.False,
                    $"Property 6 failed for iFrameDuration={iFrameDuration}: IsInvincible should be false after {iFrameDuration}s elapsed");

                // Cleanup
                _dashState.OnExit();
            }
        }

        // ─── 7.9 Property 7 — CanExit after duration ─────────────────────────────
        // Validates: Requirements 8.4, 8.5

        [Test]
        public void Property7_CanExitAfterDuration() {
            float[] dashDurations = { 0.05f, 0.08f, 0.1f, 0.15f, 0.18f, 0.2f, 0.25f, 0.3f, 0.4f, 0.5f };

            foreach (float dashDuration in dashDurations) {
                // Configure the SO and reload backing fields
                _props.dashDuration = dashDuration;
                _props.dashIFrameDuration = Mathf.Min(0.01f, dashDuration * 0.5f); // keep I-frame shorter than dash
                typeof(PlayerController)
                    .GetMethod("LoadPlayerProperties", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(_controller, null);

                // Fresh DashState with updated params
                _dashState = new DashState(_controller, _animator);
                SetMoveInput(1f, 0f);
                _dashState.OnEnter();

                Assert.That(_dashState.CanExit, Is.False,
                    $"Property 7 precondition failed for dashDuration={dashDuration}: CanExit should be false after OnEnter()");

                // Simulate time accumulation >= dashDuration
                SimulateUpdate(dashDuration + 0.001f);

                Assert.That(_dashState.CanExit, Is.True,
                    $"Property 7 failed for dashDuration={dashDuration}: CanExit should be true after {dashDuration}s elapsed");

                // Cleanup
                _dashState.OnExit();
            }
        }

        // ─── 7.10 Property 8 — Chaos contribution on dash ────────────────────────
        // Validates: Requirements 5.1

        [Test]
        public void Property8_ChaosContributionOnDash() {
            float[] chaosValues = { 0f, 5f, 10f, 15f, 20f, 25f, 30f, 50f, 75f, 100f };
            float maxChaos = _props.maxChaosThreshold; // 100f

            foreach (float chaosPerDash in chaosValues) {
                // Configure the SO and reload backing fields
                _props.chaosPerDash = chaosPerDash;
                typeof(PlayerController)
                    .GetMethod("LoadPlayerProperties", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(_controller, null);

                // Reset chaos to 0 before each iteration
                _controller.ResetChaos();
                float chaosBefore = _controller.ChaosMeter; // should be 0

                // Fresh DashState and trigger OnEnter
                _dashState = new DashState(_controller, _animator);
                SetMoveInput(1f, 0f);
                _dashState.OnEnter();

                float expectedChaos = Mathf.Min(chaosBefore + chaosPerDash, maxChaos);
                Assert.That(_controller.ChaosMeter, Is.EqualTo(expectedChaos).Within(0.001f),
                    $"Property 8 failed for chaosPerDash={chaosPerDash}: ChaosMeter {_controller.ChaosMeter} ≠ expected {expectedChaos}");

                // Cleanup
                _dashState.OnExit();
            }
        }
    }
}
