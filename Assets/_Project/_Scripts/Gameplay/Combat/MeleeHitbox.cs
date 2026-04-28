using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ThePromisedRun.Core.Interfaces;

namespace ThePromisedRun.Gameplay.Combat {
    /// <summary>
    /// Melee hitbox driven by Animation Events.
    /// Place on a child of the weapon/hand bone.
    /// Animation clips call OnHitboxActivate / OnHitboxDeactivate.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class MeleeHitbox : MonoBehaviour {
        [Header("Damage")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Events")]
        public UnityEvent<IDamageable, Vector3> OnHitConfirmed = new UnityEvent<IDamageable, Vector3>();

        private SphereCollider   _col;
        private PlayerController _owner;
        private bool             _active;

        // Track already-hit targets per swing to avoid multi-hit
        private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();

        private void Awake() {
            _col = GetComponent<SphereCollider>();
            _col.isTrigger = true;
            _owner = GetComponentInParent<PlayerController>();
            _col.enabled = false;
        }

        /// <summary>Called by Animation Event at active frame start.</summary>
        public void OnHitboxActivate() {
            _hitThisSwing.Clear();
            _active = true;
            _col.enabled = true;
        }

        /// <summary>Called by Animation Event at active frame end.</summary>
        public void OnHitboxDeactivate() {
            _active = false;
            _col.enabled = false;
        }

        private void OnTriggerEnter(Collider other) {
            if (!_active) return;
            if (((1 << other.gameObject.layer) & hitLayers.value) == 0) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;
            if (_hitThisSwing.Contains(damageable)) return;

            _hitThisSwing.Add(damageable);

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            var info = new DamageInfo(
                baseDamage,
                hitPoint,
                (other.transform.position - transform.position).normalized,
                _owner != null ? _owner.gameObject : gameObject,
                _owner != null && _owner.IsOverloaded
            );

            damageable.TakeDamage(baseDamage, info);
            OnHitConfirmed.Invoke(damageable, hitPoint);
        }
    }
}
