using UnityEngine;

namespace ThePromisedRun.Gameplay.Juice {
    /// <summary>
    /// Squash and stretch effect on a Transform's localScale.
    /// Plays a target scale then recovers smoothly back to (1,1,1).
    /// </summary>
    public class SquashStretchJuice : MonoBehaviour, IJuice {
        [Header("Target")]
        [SerializeField] private Transform visual;

        [Header("Settings")]
        [SerializeField] private Vector3 targetScale = new Vector3(0.7f, 1.35f, 0.7f);
        [SerializeField] private float recoverySpeed = 12f;

        private bool _recovering;

        private void Awake() {
            if (visual == null)
                visual = transform.Find("Visual");
        }

        private void Update() {
            if (!_recovering || visual == null) return;

            visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one, recoverySpeed * Time.deltaTime);

            if ((visual.localScale - Vector3.one).sqrMagnitude < 0.0001f) {
                visual.localScale = Vector3.one;
                _recovering = false;
            }
        }

        public void Play() {
            if (visual == null) return;
            visual.localScale = targetScale;
            _recovering = true;
        }
    }
}
