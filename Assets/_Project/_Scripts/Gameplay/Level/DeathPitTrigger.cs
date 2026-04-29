using UnityEngine;

namespace ThePromisedRun.Gameplay.Level {
    /// <summary>
    /// Death pit trigger — kills player on contact, triggers respawn.
    /// </summary>
    public class DeathPitTrigger : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player")) return;

            var playerHealth = other.GetComponent<Combat.PlayerHealth>();
            if (playerHealth != null) {
                // Instant kill — deal massive damage
                playerHealth.TakeDamage(9999f, new Combat.DamageInfo());
                Debug.Log("[DeathPit] Player fell into pit!");
            }
        }
    }
}
