using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Core.Systems;

namespace ThePromisedRun.Trap
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class ContactDamage : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Damage")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Environmental;
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("Options")]
        [Tooltip("If true, this object can only deal damage once.")]
        [SerializeField] private bool singleUse = false;

        [Tooltip("Minimum time between damage applications (even if target exits/enters quickly).")]
        [SerializeField] private float cooldown = 0f;

        [Tooltip("Optional delay after enabling/spawn before damage can be applied.")]
        [SerializeField] private float armDelay = 0f;

        #endregion

        #region Private Fields

        private Collider _collider;
        private float _cooldownTimer;
        private float _armTimer;
        private bool _used;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (_collider != null && !_collider.isTrigger)
                Debug.LogWarning($"{nameof(ContactDamage)} on '{name}' expects its Collider to be marked as Is Trigger.");
        }

        private void OnEnable()
        {
            _cooldownTimer = 0f;
            _armTimer = armDelay;
            _used = false;
        }

        private void Update()
        {
            if (_armTimer > 0f)
                _armTimer -= Time.deltaTime;

            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_used && singleUse)
                return;

            if (_armTimer > 0f)
                return;

            if (_cooldownTimer > 0f)
                return;

            if (other == null)
                return;

            if (((1 << other.gameObject.layer) & targetLayers.value) == 0)
                return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
                return;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = (other.transform.position - transform.position).normalized;
            if (hitNormal.sqrMagnitude <= 0.0001f)
                hitNormal = transform.forward;

            DamageSystem.ApplyDamage(damageable, damage, gameObject, hitPoint, hitNormal, damageType);

            _cooldownTimer = cooldown;
            if (singleUse)
                _used = true;
        }

        #endregion
    }
}

