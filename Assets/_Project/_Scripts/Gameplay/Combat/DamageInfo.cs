using UnityEngine;

namespace ThePromisedRun.Gameplay.Combat {
    /// <summary>
    /// Data passed with a damage event.
    /// </summary>
    public struct DamageInfo {
        public float     Amount;
        public Vector3   HitPoint;
        public Vector3   HitNormal;
        public GameObject Attacker;
        public bool      IsOverloadBoosted; // damage × multiplier during overload

        public DamageInfo(float amount, Vector3 hitPoint, Vector3 hitNormal,
                          GameObject attacker, bool overloadBoosted = false) {
            Amount           = amount;
            HitPoint         = hitPoint;
            HitNormal        = hitNormal;
            Attacker         = attacker;
            IsOverloadBoosted = overloadBoosted;
        }
    }
}
