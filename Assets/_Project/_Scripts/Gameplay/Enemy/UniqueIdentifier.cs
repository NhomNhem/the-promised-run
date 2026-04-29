using UnityEngine;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Attach to prefabs that should be spawned uniquely by tooling (MCP/editor helpers).
    /// Stores a persistent key that editor helpers can use to detect duplicates.
    /// </summary>
    public class UniqueIdentifier : MonoBehaviour {
        [Tooltip("A stable unique key for tooling (e.g. prefab name or GUID).")]
        public string Key;
    }


}

