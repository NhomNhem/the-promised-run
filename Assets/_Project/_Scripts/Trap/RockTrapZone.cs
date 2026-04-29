using UnityEngine;

namespace ThePromisedRun.Trap
{
    [RequireComponent(typeof(Collider))]
    public sealed class RockTrapZone : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Filter")]
        [SerializeField] private LayerMask playerLayers = ~0;

        #endregion

        #region Public Properties

        public bool IsPlayerInside { get; private set; }

        #endregion

        #region Private Fields

        private Collider _collider;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (_collider == null)
                throw new MissingComponentException($"{nameof(RockTrapZone)} on '{name}' requires a Collider.");

            if (!_collider.isTrigger)
                _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
                return;

            if (((1 << other.gameObject.layer) & playerLayers.value) == 0)
                return;

            IsPlayerInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null)
                return;

            if (((1 << other.gameObject.layer) & playerLayers.value) == 0)
                return;

            IsPlayerInside = false;
        }

        #endregion
    }
}

