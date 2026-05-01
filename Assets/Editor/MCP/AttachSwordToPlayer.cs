using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility — gán sword_1handed vào hand bone của Player prefab.
/// Menu: MCP/Attach Sword To Player
/// </summary>
public class AttachSwordToPlayer : EditorWindow
{
    [MenuItem("MCP/Attach Sword To Player")]
    public static void AttachSword()
    {
        const string playerPrefabPath = "Assets/_Project/_Prefabs/Characters/Player.prefab";
        const string swordFbxPath     = "Assets/_Project/_Art/Characters/KayKit/Characters/KayKit - Adventurers (for Unity)/Models/Accessories/sword_1handed.fbx";
        const string materialPath     = "Assets/_Project/_Art/Characters/KayKit/Characters/KayKit - Adventurers (for Unity)/Materials/knight.mat";

        // Load Player prefab
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        if (playerPrefab == null) {
            Debug.LogError($"[AttachSword] Player prefab not found at: {playerPrefabPath}");
            return;
        }

        // Load sword mesh (sub-asset từ FBX)
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(swordFbxPath);
        Mesh swordMesh = null;
        foreach (var asset in allAssets) {
            if (asset is Mesh m) {
                swordMesh = m;
                Debug.Log($"[AttachSword] Found mesh: {m.name}");
                break;
            }
        }

        if (swordMesh == null) {
            Debug.LogError($"[AttachSword] Sword mesh not found in: {swordFbxPath}");
            return;
        }

        // Load material
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null) {
            Debug.LogWarning($"[AttachSword] Material not found at: {materialPath} — sword will have no material.");
        }

        // Mở prefab để edit
        string prefabAssetPath = AssetDatabase.GetAssetPath(playerPrefab);
        using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabAssetPath)) {
            var root = editScope.prefabContentsRoot;

            // Tìm hand bone — ưu tiên handslot.r, fallback hand.r
            Transform handBone = FindBone(root.transform, "handslot.r")
                              ?? FindBone(root.transform, "hand.r")
                              ?? FindBone(root.transform, "Hand_R")
                              ?? FindBone(root.transform, "RightHand");

            if (handBone == null) {
                // Log tất cả transforms để debug
                var allT = root.GetComponentsInChildren<Transform>(true);
                string names = "";
                foreach (var t in allT) names += t.name + "\n";
                Debug.LogError($"[AttachSword] Hand bone not found. All transforms:\n{names}");
                return;
            }

            Debug.Log($"[AttachSword] Attaching sword to bone: {handBone.name}");

            // Xóa Sword cũ nếu đã có
            var existingSword = handBone.Find("Sword");
            if (existingSword != null) {
                Object.DestroyImmediate(existingSword.gameObject);
                Debug.Log("[AttachSword] Removed existing Sword.");
            }

            // Tạo Sword GameObject
            var swordGO = new GameObject("Sword");
            swordGO.transform.SetParent(handBone, false);

            // Thêm MeshFilter
            var meshFilter = swordGO.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = swordMesh;

            // Thêm MeshRenderer
            var meshRenderer = swordGO.AddComponent<MeshRenderer>();
            if (material != null)
                meshRenderer.sharedMaterial = material;

            // Điều chỉnh transform — kiếm nằm dọc theo trục tay
            swordGO.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            swordGO.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            swordGO.transform.localScale    = Vector3.one;

            Debug.Log($"[AttachSword] ✅ Sword attached to {handBone.name} in Player prefab.");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[AttachSword] ✅ Player prefab saved.");
    }

    private static Transform FindBone(Transform root, string boneName)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) {
            if (t.name.Equals(boneName, System.StringComparison.OrdinalIgnoreCase))
                return t;
        }
        return null;
    }
}
