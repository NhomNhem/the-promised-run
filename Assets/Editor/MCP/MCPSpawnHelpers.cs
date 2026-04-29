using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor helpers intended to be used by human operators or an MCP server
/// running against the Unity Editor. These helpers provide a single-entry
/// spawn API that avoids creating duplicate instances when a prefab has
/// an attached `UniqueIdentifier` component.
///
/// Usage (Editor): Window > MCP > Spawn Unique Enemy
/// Usage (script): call MCPSpawnHelpers.SpawnUniqueByPrefabPath(prefabAssetPath, position, uniqueKey)
/// </summary>
public static class MCPSpawnHelpers {
    [MenuItem("MCP/Spawn Unique Enemy (select prefab)")]
    public static void SpawnUniqueFromSelection() {
        var obj = Selection.activeObject as GameObject;
        if (obj == null) {
            Debug.LogError("Select a prefab asset to spawn.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(obj);
        // Use prefab name as key fallback
        string key = obj.name;
        SpawnUniqueByPrefabPath(path, Vector3.zero, key);
    }

    /// <summary>
    /// Spawn a prefab by asset path if an instance with the same unique key
    /// does not already exist in the current scene. Returns the existing or
    /// newly created GameObject.
    /// Note: This method is editor-only and uses AssetDatabase.
    /// </summary>
    public static GameObject SpawnUniqueByPrefabPath(string prefabAssetPath, Vector3 position, string uniqueKey) {
        if (string.IsNullOrEmpty(prefabAssetPath)) {
            Debug.LogError("Prefab asset path is null or empty.");
            return null;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
        if (prefab == null) {
            Debug.LogError($"Prefab not found at path: {prefabAssetPath}");
            return null;
        }

        // Search for existing instance with UniqueIdentifier.Key == uniqueKey
        var existing = Object.FindObjectsByType<ThePromisedRun.Gameplay.Enemy.UniqueIdentifier>(FindObjectsSortMode.InstanceID)
            .FirstOrDefault(u => u != null && u.Key == uniqueKey);

        if (existing != null) {
            Debug.Log($"MCPSpawnHelpers: Found existing instance for key '{uniqueKey}' -> {existing.gameObject.name}");
            return existing.gameObject;
        }

        // Instantiate prefab into scene
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (go == null) {
            Debug.LogError("Failed to instantiate prefab.");
            return null;
        }

        go.transform.position = position;

        // Ensure UniqueIdentifier exists and has key
        var uid = go.GetComponent<ThePromisedRun.Gameplay.Enemy.UniqueIdentifier>();
        if (uid == null) {
            uid = go.AddComponent<ThePromisedRun.Gameplay.Enemy.UniqueIdentifier>();
            uid.Key = uniqueKey;
        } else if (string.IsNullOrEmpty(uid.Key)) {
            uid.Key = uniqueKey;
        }

        // Mark scene dirty so changes are saved by user
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);

        Debug.Log($"MCPSpawnHelpers: Spawned unique prefab '{prefab.name}' as '{go.name}' with key '{uniqueKey}'");
        return go;
    }
}

