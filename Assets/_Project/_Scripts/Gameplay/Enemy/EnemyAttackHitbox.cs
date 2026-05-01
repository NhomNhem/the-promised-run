using UnityEngine;
using UnityEngine.Events;
using ThePromisedRun.Core.Interfaces;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.Enemy {
    /// <summary>
    /// Enemy melee attack hitbox — sphere trigger activated by EnemyAIController.
    /// Deals damage to IDamageable targets on the Player layer.
    /// Activated/deactivated by EnemyAIController.AttackState.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class EnemyAttackHitbox : MonoBehaviour {
        [Header("Damage")]
        [SerializeField] private float _damage = 6f;
        [SerializeField] private LayerMask _targetLayers;

        [Header("Events")]
        public UnityEvent OnHit = new UnityEvent();

        private SphereCollider _col;
        private bool _active;

        private void Awake() {
            _col = GetComponent<SphereCollider>();
            _col.isTrigger = true;
            Deactivate();
        }

        public void Activate() {
            _active = true;
            _col.enabled = true;
        }

        public void Deactivate() {
            _active = false;
            _col.enabled = false;
        }

        private void OnTriggerEnter(Collider other) {
            if (!_active) return;
            if (((1 << other.gameObject.layer) & _targetLayers) == 0) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;

            var info = new DamageInfo(
                _damage,
                other.ClosestPoint(transform.position),
                (other.transform.position - transform.position).normalized,
                transform.root.gameObject,
                false
            );
            damageable.TakeDamage(_damage, info);
            OnHit.Invoke();
            Deactivate(); // one hit per swing
        }
    }
}
