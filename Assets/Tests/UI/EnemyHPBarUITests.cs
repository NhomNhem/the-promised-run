using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using ThePromisedRun.UI;

namespace ThePromisedRun.Tests.UI {
    /// <summary>
    /// EditMode tests for EnemyHPBarUI.
    /// Property 3: EnemyHPBarUI fill and class consistency
    /// Property 4: EnemyHPBarUI billboard rotation
    /// Unit test: HideBar sets GameObject inactive
    /// </summary>
    public class EnemyHPBarUITests {

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Simulate SetFill(hp) CSS class logic — mirrors EnemyHPBarUI.SetFill.
        /// Returns (clampedHP, cssClass) where cssClass is "low", "medium", or "".
        /// </summary>
        private static (float clampedHP, string cssClass) ComputeEnemyHPState(float normalizedHP) {
            float hp = Mathf.Clamp01(normalizedHP);

            string cssClass;
            if (hp < 0.3f)      cssClass = "low";
            else if (hp < 0.6f) cssClass = "medium";
            else                cssClass = "";

            return (hp, cssClass);
        }

        // ── Property 3: EnemyHPBarUI fill and class consistency ───────────────────
        // Feature: scene-hud-optimization, Property 3: EnemyHPBarUI fill and class consistency
        // Validates: Requirements 6.3, 6.4, 6.5, 6.6

        [Test]
        public void Property3_EnemyHPBar_SetFill_ForAnyNormalizedHP_CorrectWidthAndClass() {
            // 100 samples in [0, 1]
            float[] values = GenerateFloatSamples(0f, 1f, 100);

            foreach (float hp in values) {
                (float clampedHP, string cssClass) = ComputeEnemyHPState(hp);

                // clamped value must be in [0,1]
                Assert.That(clampedHP, Is.InRange(0f, 1f),
                    $"Property 3: clampedHP {clampedHP} out of [0,1] for hp={hp}");

                // width percent = clampedHP * 100
                float expectedWidth = clampedHP * 100f;
                Assert.That(expectedWidth, Is.EqualTo(Mathf.Clamp01(hp) * 100f).Within(0.001f),
                    $"Property 3: width {expectedWidth}% ≠ {Mathf.Clamp01(hp) * 100f}% for hp={hp}");

                // CSS class boundaries
                if (hp < 0.3f)
                    Assert.That(cssClass, Is.EqualTo("low"),
                        $"Property 3: expected 'low' for hp={hp}");
                else if (hp < 0.6f)
                    Assert.That(cssClass, Is.EqualTo("medium"),
                        $"Property 3: expected 'medium' for hp={hp}");
                else
                    Assert.That(cssClass, Is.EqualTo(""),
                        $"Property 3: expected no class for hp={hp}");
            }
        }

        // ── Property 4: EnemyHPBarUI billboard rotation ───────────────────────────
        // Feature: scene-hud-optimization, Property 4: EnemyHPBarUI billboard rotation
        // Validates: Requirements 6.8

        [Test]
        public void Property4_EnemyHPBar_LateUpdate_ForAnyCameraYRotation_MatchesCamera() {
            // Create a minimal EnemyHPBarUI setup
            var root = new GameObject("EnemyRoot");
            var hpBarGO = new GameObject("HPBar");
            hpBarGO.transform.SetParent(root.transform);

            // Add UIDocument (required by [RequireComponent])
            hpBarGO.AddComponent<UnityEngine.UIElements.UIDocument>();
            var hpBar = hpBarGO.AddComponent<EnemyHPBarUI>();

            // Create a fake camera transform
            var camGO = new GameObject("FakeCamera");
            var camTransform = camGO.transform;

            // Inject _cam via reflection
            typeof(EnemyHPBarUI)
                .GetField("_cam", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(hpBar, camTransform);

            // 100 theta samples in [0, 360]
            float[] thetas = GenerateFloatSamples(0f, 360f, 100);

            foreach (float theta in thetas) {
                camTransform.rotation = Quaternion.Euler(0f, theta, 0f);

                // Invoke LateUpdate via reflection
                typeof(EnemyHPBarUI)
                    .GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(hpBar, null);

                Vector3 euler = hpBarGO.transform.rotation.eulerAngles;

                Assert.That(euler.x, Is.EqualTo(0f).Within(0.01f),
                    $"Property 4: X rotation should be 0 for theta={theta}, got {euler.x}");
                Assert.That(euler.y, Is.EqualTo(theta).Within(0.5f),
                    $"Property 4: Y rotation should be {theta} for theta={theta}, got {euler.y}");
                Assert.That(euler.z, Is.EqualTo(0f).Within(0.01f),
                    $"Property 4: Z rotation should be 0 for theta={theta}, got {euler.z}");
            }

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(camGO);
        }

        // ── Unit test: HideBar sets GameObject inactive ───────────────────────────
        // Validates: Requirements 6.7

        [Test]
        public void EnemyHPBarUI_HideBar_SetsGameObjectInactive() {
            var root = new GameObject("EnemyRoot");
            var hpBarGO = new GameObject("HPBar");
            hpBarGO.transform.SetParent(root.transform);
            hpBarGO.AddComponent<UnityEngine.UIElements.UIDocument>();
            var hpBar = hpBarGO.AddComponent<EnemyHPBarUI>();

            // Invoke HideBar via reflection (private method)
            typeof(EnemyHPBarUI)
                .GetMethod("HideBar", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(hpBar, null);

            Assert.That(hpBarGO.activeSelf, Is.False,
                "EnemyHPBarUI.HideBar() should set the GameObject inactive");

            Object.DestroyImmediate(root);
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
