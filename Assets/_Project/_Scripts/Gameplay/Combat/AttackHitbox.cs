using UnityEngine;
using UnityEngine.Events;
using ThePromisedRun.Gameplay.Juice;

namespace ThePromisedRun.Gameplay.Combat {
    /// <summary>
    /// Sphere trigger hitbox for melee attacks.
    /// Enable/disable via animation events or AttackController.
    /// Calls IDamageable.TakeDamage on hit and fires juice effects.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class AttackHitbox : MonoBehaviour {
        [Header("Damage")]
        [SerializeField] private float baseDamage           = 10f;
        [SerializeField] private float overloadDamageMultiplier = 3f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Juice")]
        [SerializeField] private PlayerJuice juice;
        [SerializeField] private ParticleSystem hitParticle;

        [Header("Events")]
        public UnityEvent<IDamageable> OnHitTarget = new UnityEvent<IDamageable>();

        private SphereCollider  _collider;
        private PlayerController _owner;
        private bool _active;

        private void Awake() {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
            _owner = GetComponentInParent<PlayerController>();

            if (juice == null)
                juice = GetComponentInParent<PlayerJuice>();

            Deactivate();
        }

        /// <summary>Enable hitbox for this attack swing.</summary>
        public void Activate() {
            _active = true;
            _collider.enabled = true;
        }

        /// <summary>Disable hitbox — call at end of attack active frame.</summary>
        public void Deactivate() {
            _active = false;
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other) {
            if (!_active) return;
            if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;

            bool overloaded = _owner != null && _owner.IsOverloaded;
            float damage = baseDamage * (overloaded ? overloadDamageMultiplier : 1f);

            var info = new DamageInfo(
                damage,
                other.ClosestPoint(transform.position),
                (other.transform.position - transform.position).normalized,
                _owner != null ? _owner.gameObject : gameObject,
                overloaded
            );

            damageable.TakeDamage(damage, info);
            OnHitTarget.Invoke(damageable);

            // Juice on confirmed hit
            juice?.OnAttackHit();
            SpawnHitParticle(info.HitPoint, info.HitNormal);

            // Deactivate after first hit to avoid multi-hit per swing
            Deactivate();
        }

        private void SpawnHitParticle(Vector3 position, Vector3 normal) {
            if (hitParticle == null) return;
            hitParticle.transform.position = position;
            hitParticle.transform.rotation = Quaternion.LookRotation(normal);
            hitParticle.Play();
        }
    }
}
