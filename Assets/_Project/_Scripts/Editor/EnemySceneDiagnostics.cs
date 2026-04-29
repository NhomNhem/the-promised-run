using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using ThePromisedRun.Gameplay.Enemy;
using ThePromisedRun.Gameplay.Enemy.AI;

namespace ThePromisedRun.Editor {
    /// <summary>
    /// Editor utility to print Enemy / NavMesh / AI diagnostics for the current scene.
    /// Use menu: Tools -> Enemy Diagnostics -> Print NavMesh Status
    /// Or run in batch mode with -executeMethod ThePromisedRun.Editor.EnemySceneDiagnostics.RunDiagnosticsFromBatch
    /// </summary>
    public static class EnemySceneDiagnostics {
        [MenuItem("Tools/Enemy Diagnostics/Print NavMesh Status", priority = 1000)]
        public static void PrintNavMeshStatus() {
            var enemies = Object.FindObjectsOfType<Enemy>();
            if (enemies == null || enemies.Length == 0) {
                Debug.Log("[EnemyDiagnostics] No Enemy instances found in the current scene.");
                return;
            }

            Debug.Log($"[EnemyDiagnostics] Found {enemies.Length} Enemy(s) in scene");

            foreach (var e in enemies) {
                if (e == null) continue;

                var go = e.gameObject;
                var agent = go.GetComponent<NavMeshAgent>();
                var rb = go.GetComponent<Rigidbody>();
                var ai = go.GetComponent<EnemyAIController>();

                string agentStr = agent == null ? "NoAgent" : (agent.isOnNavMesh ? "OnNavMesh" : "NotOnNavMesh");
                string rbStr = rb == null ? "NoRigidbody" : (rb.isKinematic ? "Rigidbody(Kinematic)" : "Rigidbody(Active)");
                string aiStr = ai == null ? "NoAIController" : (ai.IsInitialized ? $"AI(Init:{ai.GetCurrentStateName()})" : "AI(NotInit)");
                string targetStr = "NoTarget";
                try {
                    if (e.HasTarget && e.CurrentTarget != null) {
                        var tgo = ((MonoBehaviour)e.CurrentTarget).gameObject;
                        targetStr = $"Target={tgo.name} pos={tgo.transform.position}";
                    }
                } catch {
                    targetStr = "Target=ErrorReading";
                }

                Debug.Log($"[EnemyDiagnostics] {go.name} - pos={go.transform.position} nav={agentStr} rb={rbStr} ai={aiStr} detectionRadius={e.DetectionRadius} loseTargetTime={e.TimeSinceLastSeenTarget}/{e.TimeSinceLastSeenTarget}");
                Debug.Log($"[EnemyDiagnostics]   -> {targetStr}");
            }
        }

        // Entry point for batchmode / automated runs
        public static void RunDiagnosticsFromBatch() {
            // Ensure editor is ready
            PrintNavMeshStatus();
            // When running in batchmode, also force an EditorApplication quit so Unity process ends cleanly
            #if UNITY_EDITOR
            if (Application.isBatchMode) {
                Debug.Log("[EnemyDiagnostics] Running in batchmode - quitting Editor after diagnostics.");
                EditorApplication.Exit(0);
            }
            #endif
        }
    }
}

