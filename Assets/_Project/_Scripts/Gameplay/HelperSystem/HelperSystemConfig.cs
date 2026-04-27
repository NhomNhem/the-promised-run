using UnityEngine;

namespace ThePromisedRun.Gameplay.HelperSystem {
    /// <summary>
    /// Tuning data for the Helper System.
    /// Adjust in Inspector — no code changes needed for balancing.
    /// </summary>
    [CreateAssetMenu(fileName = "HelperSystemConfig", menuName = "ThePromisedRun/Helper System Config")]
    public class HelperSystemConfig : ScriptableObject {
        [Header("Popup Timing")]
        [Tooltip("Min seconds between popup spawns")]
        public float minInterval = 3f;
        [Tooltip("Max seconds between popup spawns")]
        public float maxInterval = 7f;

        [Header("Chaos per Event")]
        [Tooltip("Chaos added each time a popup spawns")]
        public float chaosOnPopupSpawn  = 8f;
        [Tooltip("Chaos added when popup covers player (obstructs view)")]
        public float chaosOnPlayerObstructed = 12f;
        [Tooltip("Chaos added when player fails due to popup interference")]
        public float chaosOnPlayerFail  = 20f;

        [Header("Aggressiveness Scaling")]
        [Tooltip("Multiplier applied to chaos amounts as level progresses (1 = normal)")]
        [Range(1f, 3f)]
        public float aggressivenessMultiplier = 1f;
    }
}
