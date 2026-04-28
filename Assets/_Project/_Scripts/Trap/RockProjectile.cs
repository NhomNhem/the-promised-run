using UnityEngine;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Trap
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RockProjectile : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Damage")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private float hitGraceTime = 0.1f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Lifetime")]
        [SerializeField] private bool useLifeTime;
        [SerializeField] private float lifeTime = 6f;

        [Header("Motion")]
        [SerializeField] private bool faceMoveDirection = true;
        [SerializeField] private bool useGravity = false;
        [SerializeField] private bool lockVerticalDirection = false;

        #endregion

        #region Private Fields

        private Rigidbody _rb;
        private float _timer;
        private float _hitGraceTimer;
        private Collider _collider;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _timer = 0f;
            _hitGraceTimer = hitGraceTime;

            // Unity limitation: non-kinematic Rigidbody cannot use a concave (non-convex) MeshCollider.
            // Many imported rocks default to MeshCollider (convex = false) which triggers runtime errors.
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null && !meshCollider.convex)
                meshCollider.convex = true;

            // Prevent physics from making the rock spin wildly and change its perceived direction.
            if (_rb != null)
                _rb.maxAngularVelocity = 0f;
        }

        private void Update()
        {
            if (useLifeTime)
            {
                _timer += Time.deltaTime;
                if (_timer >= lifeTime)
                    Destroy(gameObject);
            }

            if (_hitGraceTimer > 0f)
                _hitGraceTimer -= Time.deltaTime;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null)
                return;

            // Common case: the rock spawns intersecting the trap collider/ground and instantly collides.
            // Give it a tiny grace period so it can exit the spawn area.
            if (_hitGraceTimer > 0f)
                return;

            if (((1 << collision.collider.gameObject.layer) & hitMask.value) == 0)
                return;

            IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                ContactPoint contact = collision.GetContact(0);
                var info = new DamageInfo(damage, contact.point, contact.normal, gameObject);
                damageable.TakeDamage(damage, info);
            }

            if (destroyOnHit)
                Destroy(gameObject);
        }

        #endregion

        #region Public API

        public void Launch(Vector3 direction, float speed)
        {
            // Intentionally disabled: this trap now only spawns the rock.
            // Keeping the method to avoid breaking existing prefab references.
        }

        #endregion
    }
}

