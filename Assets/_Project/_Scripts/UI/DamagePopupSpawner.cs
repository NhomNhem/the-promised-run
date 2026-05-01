using UnityEngine;

namespace ThePromisedRun.UI {
    /// <summary>
    /// Singleton spawner for world-space damage popups.
    /// </summary>
    public class DamagePopupSpawner : MonoBehaviour {
        private static DamagePopupSpawner _instance;

        [Header("Positioning")]
        [SerializeField] private float _playerYOffset = 2f;
        [SerializeField] private float _enemyYOffset = 2f;
        [SerializeField] private float _bossYOffset = 3f;

        public static void Spawn(Vector3 worldPosition, float amount, DamagePopupType type) {
            if (amount <= 0f) return;

            EnsureInstance();
            if (_instance == null) return;

            _instance.SpawnInternal(worldPosition, amount, type);
        }

        private static void EnsureInstance() {
            if (_instance != null) return;

            _instance = FindFirstObjectByType<DamagePopupSpawner>();
            if (_instance != null) return;

            var go = new GameObject("DamagePopupSpawner");
            _instance = go.AddComponent<DamagePopupSpawner>();
        }

        private void SpawnInternal(Vector3 worldPosition, float amount, DamagePopupType type) {
            float yOffset = GetYOffset(type);
            Color color = GetColor(type);
            float scale = type == DamagePopupType.Boss ? 1.2f : 1f;

            var go = new GameObject("DamagePopupText");
            go.transform.position = worldPosition + new Vector3(0f, yOffset, 0f);

            var popup = go.AddComponent<DamagePopupText>();
            popup.Initialize($"-{Mathf.RoundToInt(amount)}", color, scale);
        }

        private float GetYOffset(DamagePopupType type) {
            switch (type) {
                case DamagePopupType.Player:
                    return _playerYOffset;
                case DamagePopupType.Boss:
                    return _bossYOffset;
                default:
                    return _enemyYOffset;
            }
        }

        private static Color GetColor(DamagePopupType type) {
            switch (type) {
                case DamagePopupType.Player:
                    return new Color(1f, 0.75f, 0.2f);
                case DamagePopupType.Boss:
                    return new Color(1f, 0.35f, 0.35f);
                default:
                    return new Color(1f, 1f, 1f);
            }
        }
    }
}
