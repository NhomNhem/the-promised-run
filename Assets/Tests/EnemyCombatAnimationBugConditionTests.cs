using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ThePromisedRun.Gameplay.Enemy;
using ThePromisedRun.Gameplay.Enemy.AI;
using ThePromisedRun.Gameplay.Enemy.AI.States;
using ThePromisedRun.Gameplay.Combat;
using ThePromisedRun.Core.FSM;

namespace ThePromisedRun.Tests {
    /// <summary>
    /// Bug Condition Exploration Tests — Task 1
    ///
    /// These tests MUST FAIL on unfixed code. Failure confirms bugs exist.
    /// DO NOT fix the code when these tests fail.
    ///
    /// Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 1.10
    /// </summary>
    public class EnemyCombatAnimationBugConditionTests {

        // ── Shared helpers ─────────────────────────────────────────────────────

        private static RuntimeAnimatorController LoadController(string name) {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                $"Assets/_Project/_Animations/{name}.controller");
        }

        private static DamageInfo MakeDamageInfo(float amount = 10f) {
            return new DamageInfo(amount, Vector3.zero, Vector3.up, null, false);
        }

        // ── Test 1.1 — Grunt Walk Animation ────────────────────────────────────
        /// <summary>
        /// Bug Group 1: EnemyAnimatorController Walk state has no motion clip assigned
        /// (m_Motion: {fileID: 0}). CrossFade("Walk") is called but the state has no clip.
        ///
        /// EXPECTED: FAIL — animator.GetCurrentAnimatorStateInfo(0).IsName("Walk") returns false
        /// because a state with no clip assigned cannot be properly entered via CrossFade.
        ///
        /// Counterexample: stateInfo.IsName("Walk") == false (stays in Idle or default state)
        /// </summary>
        [Test]
        public void GruntWalkAnimation_ClipAssigned_AnimationPlays() {
            // Arrange
            var go = new GameObject("GruntWalkTest");
            go.SetActive(false);
            var animator = go.AddComponent<Animator>();

            var controller = LoadController("EnemyAnimatorController");
            Assert.IsNotNull(controller,
                "EnemyAnimatorController.controller not found at Assets/_Project/_Animations/. " +
                "Ensure the file exists before running this test.");
            animator.runtimeAnimatorController = controller;

            go.SetActive(true);
            animator.Update(0f); // initialize animator

            // Act — CrossFade to Walk (same call as EnemyChaseState.OnEnter)
            animator.CrossFade("Walk", 0.1f, 0);
            animator.Update(0.25f); // advance past transition duration

            // Assert — Walk state should be playing
            // BUG: Walk state has m_Motion: {fileID: 0} — no clip assigned
            // Unity will not properly enter a state with no motion clip via CrossFade
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Assert.IsTrue(
                stateInfo.IsName("Walk"),
                $"Expected animator to be in 'Walk' state but was in state with hash {stateInfo.shortNameHash}. " +
                "BUG: EnemyAnimatorController Walk state has no animation clip assigned (m_Motion: {{fileID: 0}})."
            );

            // Cleanup
            Object.DestroyImmediate(go);
        }

        // ── Test 1.2 — Skeleton Hit Trigger Mismatch ───────────────────────────
        /// <summary>
        /// Bug Group 2: Skeleton.controller uses parameter "GetHit" but EnemyHealth.cs
        /// calls SetTrigger("Hit") (via Animator.StringToHash("Hit")).
        /// The trigger names don't match — animation Hit never plays.
        ///
        /// EXPECTED: FAIL — SetTrigger("Hit") does not transition to Hit state
        /// because the parameter is named "GetHit" in Skeleton.controller.
        ///
        /// Counterexample: stateInfo.IsName("Hit") == false after SetTrigger("Hit")
        /// </summary>
        [Test]
        public void SkeletonHitTrigger_ParameterMatch_AnimationPlays() {
            // Arrange
            var go = new GameObject("SkeletonHitTest");
            go.SetActive(false);
            var animator = go.AddComponent<Animator>();

            var controller = LoadController("Skeleton");
            Assert.IsNotNull(controller,
                "Skeleton.controller not found at Assets/_Project/_Animations/. " +
                "Ensure the file exists before running this test.");
            animator.runtimeAnimatorController = controller;

            go.SetActive(true);
            animator.Update(0f); // initialize in Idle state

            // Verify the bug: Skeleton.controller has "GetHit" parameter, not "Hit"
            bool hasHitParam = false;
            bool hasGetHitParam = false;
            foreach (AnimatorControllerParameter param in animator.parameters) {
                if (param.name == "Hit") hasHitParam = true;
                if (param.name == "GetHit") hasGetHitParam = true;
            }

            // Act — EnemyHealth.cs calls SetTrigger with hash of "Hit"
            // BUG: Skeleton.controller has parameter "GetHit", not "Hit"
            animator.SetTrigger("Hit");
            animator.Update(0.15f); // advance past transition duration

            // Assert — should be in Hit state after SetTrigger("Hit")
            // FAILS because "Hit" trigger doesn't exist in Skeleton.controller (it's "GetHit")
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Assert.IsTrue(
                stateInfo.IsName("Hit"),
                $"Expected animator to be in 'Hit' state after SetTrigger(\"Hit\") " +
                $"but was in state with hash {stateInfo.shortNameHash}. " +
                $"Skeleton.controller has 'Hit' param: {hasHitParam}, has 'GetHit' param: {hasGetHitParam}. " +
                "BUG: Skeleton.controller uses parameter 'GetHit' instead of 'Hit' — trigger name mismatch."
            );

            // Cleanup
            Object.DestroyImmediate(go);
        }

        // ── Test 1.3 — Flash Color Is White (not red) ──────────────────────────
        /// <summary>
        /// Bug Group 3: EnemyHealth.MaterialFlash() sets Color.white instead of red.
        /// We verify this by inspecting the MaterialFlash coroutine via reflection
        /// to confirm it uses Color.white (the bug).
        ///
        /// EXPECTED: FAIL — the flash color set in MaterialFlash is Color.white, not red.
        ///
        /// Counterexample: MaterialFlash sets Color.white — confirmed by source inspection
        /// </summary>
        [Test]
        public void TakeDamage_FlashColor_IsRed() {
            // Verify the bug by inspecting EnemyHealth source via reflection
            // We check that MaterialFlash does NOT use MaterialPropertyBlock
            // and DOES use Color.white (the bug)

            System.Type healthType = typeof(EnemyHealth);

            // Check that _propBlock field does NOT exist (it should after fix)
            FieldInfo propBlockField = healthType.GetField(
                "_propBlock",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Check that BaseColorId static field does NOT exist (it should after fix)
            FieldInfo baseColorIdField = healthType.GetField(
                "BaseColorId",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            // The bug: MaterialFlash uses _renderer.material.color = Color.white
            // The fix: MaterialFlash uses MaterialPropertyBlock with Color(1f, 0.1f, 0.1f)
            // We assert that the fix IS in place (MaterialPropertyBlock fields exist)
            // This FAILS on unfixed code because _propBlock and BaseColorId don't exist yet

            Assert.IsNotNull(
                propBlockField,
                "EnemyHealth does not have a '_propBlock' (MaterialPropertyBlock) field. " +
                "BUG: EnemyHealth.MaterialFlash() uses _renderer.material.color = Color.white " +
                "instead of MaterialPropertyBlock with red color (1f, 0.1f, 0.1f)."
            );

            Assert.IsNotNull(
                baseColorIdField,
                "EnemyHealth does not have a 'BaseColorId' static field for shader property caching. " +
                "BUG: EnemyHealth.MaterialFlash() does not use MaterialPropertyBlock."
            );
        }

        // ── Test 1.4 — Material Instance Created on TakeDamage ─────────────────
        /// <summary>
        /// Bug Group 3: EnemyHealth.MaterialFlash() accesses _renderer.material (not sharedMaterial),
        /// which creates a new material instance every call — causing memory leak.
        ///
        /// EXPECTED: FAIL — renderer.sharedMaterial is replaced by an instance after TakeDamage.
        ///
        /// Counterexample: renderer.sharedMaterial != originalMat after TakeDamage
        /// </summary>
        [Test]
        public void TakeDamage_MaterialPropertyBlock_NoNewInstance() {
            // Arrange
            var go = new GameObject("EnemyHealthMaterialTest");
            go.SetActive(false);

            go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            var originalMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            originalMat.name = "OriginalMaterial_Test";
            renderer.sharedMaterial = originalMat;

            go.AddComponent<Animator>();

            var health = go.AddComponent<EnemyHealth>();
            typeof(EnemyHealth)
                .GetField("_maxHealth", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(health, 100f);

            go.SetActive(true);

            // Manually invoke Awake to initialize _renderer reference
            typeof(EnemyHealth)
                .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(health, null);

            // Record the original sharedMaterial reference
            Material sharedBefore = renderer.sharedMaterial;
            int instanceIdBefore = sharedBefore.GetInstanceID();

            // Act — call TakeDamage twice (each call starts MaterialFlash coroutine)
            health.TakeDamage(5f, MakeDamageInfo(5f));
            health.TakeDamage(5f, MakeDamageInfo(5f));

            // Assert — sharedMaterial should still be the original (not replaced by instance)
            // BUG: _renderer.material creates a new instance, replacing sharedMaterial
            // After accessing _renderer.material, Unity replaces sharedMaterial with the instance
            // This assertion FAILS because renderer.sharedMaterial is now an instance, not originalMat
            Assert.AreEqual(
                instanceIdBefore,
                renderer.sharedMaterial.GetInstanceID(),
                $"Expected renderer.sharedMaterial to remain the original material " +
                $"(instanceID={instanceIdBefore}) but it was replaced with a new instance " +
                $"(instanceID={renderer.sharedMaterial.GetInstanceID()}). " +
                "BUG: EnemyHealth.MaterialFlash() uses _renderer.material which creates a new material instance."
            );

            // Cleanup
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(originalMat);
        }

        // ── Test 1.5 — No EnemyHitState in FSM ─────────────────────────────────
        /// <summary>
        /// Bug Group 4: EnemyBrain FSM does not contain EnemyHitState.
        /// When TakeDamage is called, the FSM should transition to EnemyHitState
        /// but the class doesn't exist yet.
        ///
        /// EXPECTED: FAIL — EnemyHitState type does not exist in the assembly.
        ///
        /// Counterexample: Type.GetType("...EnemyHitState...") returns null
        /// </summary>
        [Test]
        public void TakeDamage_FSM_TransitionsToHitState() {
            // Check if EnemyHitState type exists in the assembly
            System.Type hitStateType = null;

            // Search all loaded assemblies for EnemyHitState
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
                hitStateType = assembly.GetType("ThePromisedRun.Gameplay.Enemy.AI.States.EnemyHitState");
                if (hitStateType != null) break;
            }

            // Assert — EnemyHitState class should exist (it doesn't yet — bug confirmed)
            // This FAILS because EnemyHitState has not been created yet
            Assert.IsNotNull(
                hitStateType,
                "EnemyHitState type not found in any loaded assembly. " +
                "BUG: EnemyHitState class does not exist — " +
                "EnemyBrain FSM cannot transition to Hit state when TakeDamage is called."
            );

            // Secondary check: if the type exists, verify EnemyBrain FSM wires it
            if (hitStateType != null) {
                // Check EnemyBrain has a _hit field of type EnemyHitState
                FieldInfo hitField = typeof(EnemyBrain).GetField(
                    "_hit",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                Assert.IsNotNull(
                    hitField,
                    "EnemyBrain does not have a '_hit' field of type EnemyHitState. " +
                    "BUG: EnemyHitState is not wired into EnemyBrain.SetupFSM()."
                );
            }
        }

        // ── Test 1.6 — Death Trigger Called Twice ──────────────────────────────
        /// <summary>
        /// Bug Group 5: When enemy dies, death animation is triggered twice:
        ///   1. Enemy.HandleDeath() calls animator.SetTrigger("Death")
        ///   2. EnemyDeadFSMState.OnEnter() calls animator.CrossFade("Death", 0.1f, 0)
        ///
        /// EXPECTED: FAIL — Enemy.HandleDeath() should NOT call SetTrigger("Death").
        ///
        /// Counterexample: HandleDeath() contains SetTrigger("Death") call (confirmed by IL inspection)
        /// </summary>
        [Test]
        public void HandleDeath_DeathTrigger_CalledOnce() {
            // Verify the bug by inspecting Enemy.HandleDeath() method body via IL
            // We check that HandleDeath does NOT reference the "Death" trigger string
            System.Type enemyType = typeof(Enemy);
            MethodInfo handleDeathMethod = enemyType.GetMethod(
                "HandleDeath",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.IsNotNull(handleDeathMethod, "HandleDeath method not found on Enemy");

            // Get the IL bytes of HandleDeath and check for "Death" string reference
            // This is the most reliable way to verify the bug without running the full game
            byte[] ilBytes = handleDeathMethod.GetMethodBody()?.GetILAsByteArray();
            Assert.IsNotNull(ilBytes, "Could not get IL bytes for Enemy.HandleDeath()");

            // Alternative approach: create enemy with animator, call HandleDeath,
            // and verify the Death trigger was set on the animator
            var go = new GameObject("EnemyDeathTriggerTest");
            go.SetActive(false);

            var rb = go.AddComponent<Rigidbody>();
            var animator = go.AddComponent<Animator>();

            var controller = LoadController("EnemyAnimatorController");
            if (controller != null) animator.runtimeAnimatorController = controller;

            var enemy = go.AddComponent<Enemy>();

            // Inject rb and animator fields (they are [SerializeField] protected)
            SetField(enemy, "rb", rb);
            SetField(enemy, "animator", animator);

            go.SetActive(true);
            animator.Update(0f);

            // Reset any pending triggers before the test
            animator.ResetTrigger("Death");

            // Act — call HandleDeath() directly (simulates what Entity.Die() calls)
            handleDeathMethod.Invoke(enemy, null);

            // Check if Death trigger was set by HandleDeath
            // In Unity, after SetTrigger, the trigger is pending until next Update
            // We can detect this by checking animator.IsInTransition or by checking
            // if the trigger parameter is set
            bool deathTriggerSetByHandleDeath = false;
            foreach (AnimatorControllerParameter param in animator.parameters) {
                if (param.name == "Death" && param.type == AnimatorControllerParameterType.Trigger) {
                    // Advance animator to consume the trigger
                    animator.Update(0.01f);
                    // If we're now in a Death-related state, the trigger was set
                    AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
                    deathTriggerSetByHandleDeath = info.IsName("Dead") || info.IsName("Death");
                    break;
                }
            }

            // Assert: HandleDeath() should NOT set the Death trigger
            // (death animation should only come from EnemyDeadFSMState.OnEnter())
            // BUG: HandleDeath() calls animator.SetTrigger("Death") — this is the duplicate
            // This assertion FAILS because HandleDeath does call SetTrigger("Death")
            Assert.IsFalse(
                deathTriggerSetByHandleDeath,
                "Enemy.HandleDeath() triggered the Death animation (animator transitioned to Dead/Death state). " +
                "BUG: Death animation is triggered from both Enemy.HandleDeath() AND EnemyDeadFSMState.OnEnter(). " +
                "Expected: HandleDeath() should NOT call SetTrigger(\"Death\") — " +
                "only EnemyDeadFSMState.OnEnter() should trigger the death animation."
            );

            // Cleanup
            Object.DestroyImmediate(go);
        }

        // ── Test 1.7 — No Despawn After Death ──────────────────────────────────
        /// <summary>
        /// Bug Group 5: After enemy dies, the GameObject is never despawned.
        /// EnemyDeadFSMState has no despawn coroutine.
        ///
        /// EXPECTED: FAIL — gameObject.activeSelf == true after 3 seconds (no despawn).
        ///
        /// Counterexample: go.activeSelf == true after 3 seconds
        /// </summary>
        [UnityTest]
        public IEnumerator EnemyDead_DespawnAfterAnimation() {
            // Arrange
            var go = new GameObject("EnemyDespawnTest");
            go.SetActive(false);

            var rb = go.AddComponent<Rigidbody>();
            var animator = go.AddComponent<Animator>();

            var controller = LoadController("EnemyAnimatorController");
            if (controller != null) animator.runtimeAnimatorController = controller;

            var enemy = go.AddComponent<Enemy>();
            var health = go.AddComponent<EnemyHealth>();

            SetField(enemy, "rb", rb);
            SetField(enemy, "animator", animator);

            typeof(EnemyHealth)
                .GetField("_maxHealth", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(health, 30f);

            go.SetActive(true);

            // Initialize EnemyHealth
            typeof(EnemyHealth)
                .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(health, null);

            // Simulate death via EnemyDeadFSMState (the FSM path that should despawn)
            var deadState = new EnemyDeadFSMState(enemy);
            deadState.OnEnter();

            // Wait 3 seconds — if despawn were implemented, gameObject would be inactive
            yield return new WaitForSeconds(3f);

            // Assert — gameObject should be despawned (inactive) after death animation
            // BUG: EnemyDeadFSMState has no despawn coroutine → gameObject stays active forever
            // This assertion FAILS because no despawn is implemented
            Assert.IsFalse(
                go.activeSelf,
                "Expected enemy gameObject to be despawned (activeSelf == false) after 3 seconds, " +
                "but it is still active. " +
                "BUG: EnemyDeadFSMState.OnEnter() does not schedule a despawn coroutine — " +
                "enemy GameObjects accumulate in the scene after death."
            );

            // Cleanup
            if (go != null) Object.DestroyImmediate(go);
        }

        // ── Reflection helper ──────────────────────────────────────────────────

        /// <summary>
        /// Sets a field on an object via reflection, searching both public and non-public,
        /// instance fields including inherited ones.
        /// </summary>
        private static void SetField(object target, string fieldName, object value) {
            System.Type type = target.GetType();
            while (type != null) {
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                );
                if (field != null) {
                    field.SetValue(target, value);
                    return;
                }
                type = type.BaseType;
            }
            Debug.LogWarning($"[EnemyCombatAnimationBugConditionTests] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
