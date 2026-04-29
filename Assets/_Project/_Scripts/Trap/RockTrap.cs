using UnityEngine;

namespace ThePromisedRun.Trap
{
    public sealed class RockTrap : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Zone")]
        [SerializeField] private RockTrapZone zone;

        [Header("Trigger Fallback")]
        [Tooltip("If you don't want to use RockTrapZone, you can put a Trigger Collider on this object and enable this.")]
        [SerializeField] private LayerMask playerLayers = ~0;

        [Header("Swap")]
        [Tooltip("Object on the map that will be disabled when the trap triggers.")]
        [SerializeField] private GameObject objectToDisable;

        [Tooltip("Prefab that will be spawned at the disabled object's position.")]
        [SerializeField] private GameObject objectToSpawn;

        [Header("Spawned Animation")]
        [Tooltip("Optional: if set, this animation trigger will be fired on the spawned object's Animator.")]
        [SerializeField] private string spawnedAnimatorTrigger = string.Empty;

        [Header("Options")]
        [SerializeField] private bool singleUse = true;
        [SerializeField] private float cooldown = 0f;

        #endregion

        #region Private Fields

        private float _cooldownTimer;
        private bool _used;
        private bool _wasInside;
        private GameObject _spawnedInstance;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (zone == null)
                zone = GetComponent<RockTrapZone>();

            if (zone == null)
                Debug.LogWarning($"{nameof(RockTrap)} on '{name}' has no {nameof(RockTrapZone)} assigned.");
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            if (zone == null)
                return;

            bool isInside = zone.IsPlayerInside;
            if (isInside && !_wasInside)
                TryTrigger();

            _wasInside = isInside;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (zone != null)
                return;

            if (other == null)
                return;

            if (((1 << other.gameObject.layer) & playerLayers.value) == 0)
                return;

            TryTrigger();
        }

        #endregion

        #region Private Methods

        private void TryTrigger()
        {
            if (singleUse && _used)
                return;

            if (_cooldownTimer > 0f)
                return;

            if (objectToDisable != null)
                objectToDisable.SetActive(false);

            if (_spawnedInstance != null)
                Destroy(_spawnedInstance);

            if (objectToSpawn != null)
            {
                Transform t = objectToDisable != null ? objectToDisable.transform : transform;
                _spawnedInstance = Instantiate(objectToSpawn, t.position, t.rotation);

                RockTrap trapOnSpawned = _spawnedInstance.GetComponent<RockTrap>();
                if (trapOnSpawned != null)
                    trapOnSpawned.enabled = false;

                RockTrapZone zoneOnSpawned = _spawnedInstance.GetComponent<RockTrapZone>();
                if (zoneOnSpawned != null)
                    zoneOnSpawned.enabled = false;

                Animator spawnedAnimator = _spawnedInstance.GetComponentInChildren<Animator>(true);
                if (spawnedAnimator != null && !string.IsNullOrWhiteSpace(spawnedAnimatorTrigger))
                    spawnedAnimator.SetTrigger(spawnedAnimatorTrigger);
            }
            else
            {
                Debug.LogWarning($"{nameof(RockTrap)} on '{name}' has no objectToSpawn assigned.");
            }

            _cooldownTimer = cooldown;
            if (singleUse)
                _used = true;
        }

        #endregion
    }
}
