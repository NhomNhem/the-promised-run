using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using ThePromisedRun.UI;

namespace ThePromisedRun.Tests.UI {
    /// <summary>
    /// EditMode tests for ComboCounterUI.
    /// Property 5: ComboCounterUI display invariant
    /// Unit tests: EndCombo, SetSuspended
    ///
    /// ComboCounterUI queries VisualElements from a UIDocument at runtime.
    /// In EditMode without a full panel, _comboRoot will be null.
    /// Tests verify the internal state logic (visibility flag, text, timers)
    /// via reflection, independent of the VisualElement rendering.
    /// </summary>
    public class ComboCounterUITests {

        private GameObject    _root;
        private ComboCounterUI _combo;

        [SetUp]
        public void SetUp() {
            _root = new GameObject("HUD");
            _root.SetActive(false);

            // UIDocument required by ComboCounterUI (queries elements from it)
            _root.AddComponent<UnityEngine.UIElements.UIDocument>();
            _combo = _root.AddComponent<ComboCounterUI>();

            // Invoke OnEnable manually (root is inactive so it won't auto-fire)
            typeof(ComboCounterUI)
                .GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(_combo, null);
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_root);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private bool GetVisible() =>
            (bool)typeof(ComboCounterUI)
                .GetField("_visible", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_combo);

        private bool GetSuspended() =>
            (bool)typeof(ComboCounterUI)
                .GetField("_suspended", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_combo);

        private float GetHideTimer() =>
            (float)typeof(ComboCounterUI)
                .GetField("_hideTimer", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_combo);

        // ── Property 5: ComboCounterUI display invariant ──────────────────────────
        // Feature: scene-hud-optimization, Property 5: ComboCounterUI display invariant
        // Validates: Requirements 7.3, 7.4

        [Test]
        public void Property5_ComboCounter_SetCombo_DisplaysCorrectlyForAnyCount() {
            // 26 samples in [-5, 20]
            int[] counts = { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

            foreach (int count in counts) {
                _combo.SetCombo(count);

                bool visible = GetVisible();

                if (count > 1) {
                    Assert.That(visible, Is.True,
                        $"Property 5: combo-counter should be visible for count={count}");
                } else {
                    Assert.That(visible, Is.False,
                        $"Property 5: combo-counter should be hidden for count={count}");
                }

                // suspended must be reset on SetCombo
                Assert.That(GetSuspended(), Is.False,
                    $"Property 5: suspended should be false after SetCombo({count})");

                // hideTimer must be reset on SetCombo
                Assert.That(GetHideTimer(), Is.EqualTo(0f).Within(0.001f),
                    $"Property 5: hideTimer should be 0 after SetCombo({count})");
            }
        }

        // ── Unit test: EndCombo starts hide timer ─────────────────────────────────
        // Validates: Requirements 7.5

        [Test]
        public void ComboCounterUI_EndCombo_StartsHideTimer() {
            _combo.SetCombo(3); // make visible first
            _combo.EndCombo();

            float hideDelay = (float)typeof(ComboCounterUI)
                .GetField("_hideDelay", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_combo);

            Assert.That(GetHideTimer(), Is.EqualTo(hideDelay).Within(0.001f),
                "EndCombo() should set _hideTimer to _hideDelay");

            Assert.That(GetSuspended(), Is.False,
                "EndCombo() should clear suspended state");
        }

        // ── Unit test: SetSuspended sets flag ─────────────────────────────────────
        // Validates: Requirements 7.6

        [Test]
        public void ComboCounterUI_SetSuspended_SetsSuspendedFlag() {
            _combo.SetSuspended(true);
            Assert.That(GetSuspended(), Is.True,
                "SetSuspended(true) should set _suspended to true");

            _combo.SetSuspended(false);
            Assert.That(GetSuspended(), Is.False,
                "SetSuspended(false) should set _suspended to false");
        }
    }
}
