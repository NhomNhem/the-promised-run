using System.Collections;
using UnityEngine;

namespace ThePromisedRun.Gameplay.Juice {
    /// <summary>
    /// Juice for attack:
    /// - OnSwing: light visual punch when attack input fires
    /// - OnHit:   heavy hit-stop + strong punch when hitbox confirms contact
    /// </summary>
    public class AttackJuice : MonoBehaviour, IJuice {
        [Header("Swing (on input)")]
        [SerializeField] private Vector3 swingScale    = new Vector3(1.1f, 0.92f, 1.1f);
        [SerializeField] private float   swingRecovery = 18f;

        [Header("Hit (on confirmed contact)")]
        [SerializeField] private float hitStopDuration  = 0.07f;
        [SerializeField] private float hitStopTimeScale = 0.04f;
        [SerializeField] private Vector3 hitScale       = new Vector3(1.25f, 0.78f, 1.25f);
        [SerializeField] private float   hitRecovery    = 20f;

        [Header("References")]
        [SerializeField] private Transform visual;

        private bool  _recovering;
        private float _recoverySpeed;

        private void Awake() {
            if (visual == null) visual = transform.Find("Visual");
        }

        private void Update() {
            if (!_recovering || visual == null) return;
            visual.localScale = Vector3.Lerp(
                visual.localScale, Vector3.one, _recoverySpeed * Time.unscaledDeltaTime);
            if ((visual.localScale - Vector3.one).sqrMagnitude < 0.0001f) {
                visual.localScale = Vector3.one;
                _recovering = false;
            }
        }

        /// <summary>IJuice.Play — defaults to swing.</summary>
        public void Play() => OnSwing();

        /// <summary>Light punch on attack input.</summary>
        public void OnSwing() => ApplyScale(swingScale, swingRecovery);

        /// <summary>Heavy hit-stop + punch on confirmed hitbox contact.</summary>
        public void OnHit() {
            ApplyScale(hitScale, hitRecovery);
            // Disabled HitStop to prevent spinning issues
            // StartCoroutine(HitStop());
        }

        private void ApplyScale(Vector3 target, float recovery) {
            if (visual == null) return;
            visual.localScale = target;
            _recoverySpeed = recovery;
            _recovering = true;
        }

        private IEnumerator HitStop() {
            Time.timeScale = hitStopTimeScale;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = 1f;
        }
    }
}
