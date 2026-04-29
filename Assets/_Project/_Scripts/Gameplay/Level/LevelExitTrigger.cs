using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ThePromisedRun.Gameplay.Level {
    /// <summary>
    /// Level exit trigger — GDD §9.1.
    /// Player walks into trigger zone → level complete → load next scene.
    /// </summary>
    public class LevelExitTrigger : MonoBehaviour {
        [Header("Config")]
        [SerializeField] private string _nextSceneName = "";
        [SerializeField] private float  _transitionDelay = 1.0f;

        [Header("Visual")]
        [SerializeField] private GameObject _exitVisual;

        [Header("Events")]
        public UnityEvent OnPlayerReachedExit = new UnityEvent();

        private bool _triggered;

        private void OnTriggerEnter(Collider other) {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            OnPlayerReachedExit.Invoke();
            Debug.Log("[LevelExit] Player reached exit!");

            if (!string.IsNullOrEmpty(_nextSceneName))
                Invoke(nameof(LoadNextScene), _transitionDelay);
        }

        private void LoadNextScene() {
            SceneManager.LoadScene(_nextSceneName);
        }

        private void OnDrawGizmos() {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            var col = GetComponent<BoxCollider>();
            if (col != null)
                Gizmos.DrawCube(transform.position + col.center, col.size);
        }
    }
}
