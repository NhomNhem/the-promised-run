using UnityEngine;

namespace ThePromisedRun.Trap
{
    [RequireComponent(typeof(Collider))]
    public sealed class RockTrap : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Trigger")]
        [SerializeField] private LayerMask playerLayers = ~0;
        [SerializeField] private bool singleUse;
        [SerializeField] private float cooldown = 2f;

        [Header("Spawn")]
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private Transform spawnPoint;

        [Header("Swap")]
        [SerializeField] private GameObject oldRockToDestroy;

        [Header("Animation")]
        [SerializeField] private Animator targetAnimator;
        [SerializeField] private string triggerName = "Trigger";

        [Header("Launch")]
        [SerializeField] private float launchSpeed = 12f;
        [SerializeField] private bool useTarget;
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localLaunchDirection = Vector3.forward;

        #endregion

        #region Private Fields

        private float _cooldownTimer;
        private bool _used;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null && !triggerCollider.isTrigger)
                triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
                return;

            if (singleUse && _used)
                return;

            if (_cooldownTimer > 0f)
                return;

            if (((1 << other.gameObject.layer) & playerLayers.value) == 0)
                return;

            TriggerAnimation();

            if (rockPrefab != null && spawnPoint != null)
                SpawnRockOnly();

            if (oldRockToDestroy != null)
                Destroy(oldRockToDestroy);

            _cooldownTimer = cooldown;
            if (singleUse)
                _used = true;
        }

        #endregion

        #region Helpers

        private Vector3 ResolveLaunchDirection(Transform player)
        {
            if (useTarget)
            {
                if (target == null)
                    return (player.position - spawnPoint.position).normalized;

                return (target.position - spawnPoint.position).normalized;
            }

            Vector3 dir = localLaunchDirection;
            if (dir.sqrMagnitude <= 0.0001f)
                dir = Vector3.forward;

            // Use world-space direction so artists can type (1,0,0) etc. without depending on SpawnPoint rotation.
            return dir.normalized;
        }

        private void SpawnAndLaunch(Vector3 direction)
        {
            GameObject rockInstance = Instantiate(rockPrefab, spawnPoint.position, spawnPoint.rotation);

            // Ensure required physics components exist even if the prefab is just a mesh.
            Rigidbody rb = rockInstance.GetComponent<Rigidbody>();
            if (rb == null)
                rb = rockInstance.AddComponent<Rigidbody>();

            Collider col = rockInstance.GetComponent<Collider>();
            if (col == null)
            {
                // Fallback collider so the projectile can collide/damage.
                SphereCollider sphere = rockInstance.AddComponent<SphereCollider>();
                sphere.radius = 0.5f;
                col = sphere;
            }
            col.isTrigger = false;

            RockProjectile rock = rockInstance.GetComponent<RockProjectile>();
            if (rock == null)
            {
                Debug.LogWarning(
                    $"{nameof(RockTrap)} spawned '{rockInstance.name}' but it has no {nameof(RockProjectile)}. Adding one at runtime.");
                rock = rockInstance.AddComponent<RockProjectile>();
            }

            rockInstance.transform.rotation = spawnPoint.rotation;
        }

        private void SpawnRockOnly()
        {
            Instantiate(rockPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        private void TriggerAnimation()
        {
            if (targetAnimator == null)
                return;

            if (string.IsNullOrWhiteSpace(triggerName))
            {
                Debug.LogWarning($"{nameof(RockTrap)} on '{name}' has an empty animation trigger name.");
                return;
            }

            targetAnimator.SetTrigger(triggerName);
        }

        #endregion
    }
}
