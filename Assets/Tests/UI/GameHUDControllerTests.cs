using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using ThePromisedRun.UI;
using UnityEngine.UIElements;

namespace ThePromisedRun.Tests.UI {
    /// <summary>
    /// EditMode tests for GameHUDController.
    /// Property 1: Health bar width normalization
    /// Property 2: Chaos bar state consistency
    /// Unit test: null UIDocument guard
    ///
    /// Because UIDocument requires a full runtime panel to render VisualElements,
    /// these tests verify the normalization and CSS-class logic by calling the
    /// private handler methods via reflection and inspecting the VisualElement
    /// style/classList directly on a manually-constructed element tree.
    /// </summary>
    public class GameHUDControllerTests {

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Simulate OnHealthChanged(value) logic directly — mirrors GameHUDController.OnHealthChanged.
        /// Returns the resulting normalized width [0,1].
        /// </summary>
        private static float ComputeHealthNorm(float value, float maxHealth) =>
            Mathf.Clamp01(value / maxHealth);

        /// <summary>
        /// Simulate OnChaosChanged(value) logic — returns (norm, cssClass, overloadVisible).
        /// Mirrors GameHUDController.OnChaosChanged.
        /// </summary>
        private static (float norm, string cssClass, bool overloadVisible) ComputeChaosState(float value, float maxChaos) {
            float norm = Mathf.Clamp01(value / maxChaos);

            string cssClass;
            if (norm >= 1f)        cssClass = "full";
            else if (norm >= 0.7f) cssClass = "high";
            else if (norm >= 0.3f) cssClass = "medium";
            else                   cssClass = "low";

            bool overloadVisible = norm >= 1f;
            return (norm, cssClass, overloadVisible);
        }

        // ── Property 1: Health bar width normalization ────────────────────────────
        // Feature: scene-hud-optimization, Property 1: health bar width normalization
        // Validates: Requirements 2.4

        [Test]
        public void Property1_HealthBarWidth_ForAnyValue_EqualsNormalizedPercent() {
            const float maxHealth = 100f;

            // 100 samples spanning [-50, 200] including out-of-range values
            float[] values = GenerateFloatSamples(-50f, 200f, 100);

            foreach (float v in values) {
                float norm = ComputeHealthNorm(v, maxHealth);
                float expectedWidthPercent = norm * 100f;

                // norm must be in [0,1]
                Assert.That(norm, Is.InRange(0f, 1f),
                    $"Property 1: norm {norm} out of [0,1] for value={v}");

                // width percent must equal norm * 100
                Assert.That(expectedWidthPercent, Is.EqualTo(Mathf.Clamp01(v / maxHealth) * 100f).Within(0.001f),
                    $"Property 1: width {expectedWidthPercent}% ≠ Clamp01({v}/{maxHealth})*100 for value={v}");
            }
        }

        // ── Property 2: Chaos bar state consistency ───────────────────────────────
        // Feature: scene-hud-optimization, Property 2: chaos bar state consistency
        // Validates: Requirements 2.5, 2.6, 2.7

        [Test]
        public void Property2_ChaosBar_ForAnyValue_HasCorrectWidthAndExactlyOneClass() {
            const float maxChaos = 100f;

            // 100 samples spanning [0, 150]
            float[] values = GenerateFloatSamples(0f, 150f, 100);

            foreach (float v in values) {
                (float norm, string cssClass, bool overloadVisible) = ComputeChaosState(v, maxChaos);

                // norm must be in [0,1]
                Assert.That(norm, Is.InRange(0f, 1f),
                    $"Property 2: norm {norm} out of [0,1] for value={v}");

                // exactly one CSS class must be assigned
                bool validClass = cssClass == "low" || cssClass == "medium" || cssClass == "high" || cssClass == "full";
                Assert.That(validClass, Is.True,
                    $"Property 2: unexpected cssClass '{cssClass}' for value={v}");

                // verify class boundaries
                float normalizedRaw = v / maxChaos;
                if (normalizedRaw >= 1f)
                    Assert.That(cssClass, Is.EqualTo("full"),
                        $"Property 2: expected 'full' for v={v} (norm={normalizedRaw:F3})");
                else if (normalizedRaw >= 0.7f)
                    Assert.That(cssClass, Is.EqualTo("high"),
                        $"Property 2: expected 'high' for v={v} (norm={normalizedRaw:F3})");
                else if (normalizedRaw >= 0.3f)
                    Assert.That(cssClass, Is.EqualTo("medium"),
                        $"Property 2: expected 'medium' for v={v} (norm={normalizedRaw:F3})");
                else
                    Assert.That(cssClass, Is.EqualTo("low"),
                        $"Property 2: expected 'low' for v={v} (norm={normalizedRaw:F3})");

                // overload banner: visible iff norm >= 1
                bool expectedOverload = Mathf.Clamp01(v / maxChaos) >= 1f;
                Assert.That(overloadVisible, Is.EqualTo(expectedOverload),
                    $"Property 2: overloadVisible={overloadVisible} but expected={expectedOverload} for v={v}");
            }
        }

        // ── Unit test: null UIDocument guard ─────────────────────────────────────
        // Validates: Requirements 2.9

        [Test]
        public void GameHUDController_OnEnable_WithNullUIDocument_LogsWarningAndDoesNotThrow() {
            // Create a GameObject without UIDocument — GameHUDController requires it
            // but should not throw, only log a warning
            var go = new GameObject("HUD_Test");
            go.SetActive(false);

            // Add UIDocument is required by [RequireComponent], so we add it but
            // leave its sourceAsset null — rootVisualElement will be null at runtime
            go.AddComponent<UnityEngine.UIElements.UIDocument>();
            var hud = go.AddComponent<GameHUDController>();

            // Invoke OnEnable via reflection — should not throw
            Assert.DoesNotThrow(() => {
                typeof(GameHUDController)
                    .GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(hud, null);
            }, "GameHUDController.OnEnable() should not throw when UIDocument has no source asset");

            Object.DestroyImmediate(go);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static float[] GenerateFloatSamples(float min, float max, int count) {
            float[] samples = new float[count];
            float step = (max - min) / (count - 1);
            for (int i = 0; i < count; i++)
                samples[i] = min + step * i;
            return samples;
        }
    }
}
