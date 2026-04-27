using UnityEngine;

namespace ThePromisedRun.Gameplay.Juice {
    /// <summary>
    /// Facade that aggregates all player juice effects.
    /// PlayerController only depends on this — not on individual juice classes.
    /// </summary>
    public class PlayerJuice : MonoBehaviour {
        [Header("Takeoff")]
        [SerializeField] private SquashStretchJuice takeoffStretch;

        [Header("Landing")]
        [SerializeField] private SquashStretchJuice landSquash;
        [SerializeField] private LandImpactJuice    landImpact;

        [Header("Overload")]
        [SerializeField] private OverloadJuice overloadJuice;

        [Header("Attack")]
        [SerializeField] private AttackJuice attackJuice;

        public void OnTakeoff() => takeoffStretch?.Play();

        public void OnLand() {
            landSquash?.Play();
            landImpact?.Play();
        }

        public void OnOverloadStart() => overloadJuice?.StartOverload();
        public void OnOverloadEnd()   => overloadJuice?.StopOverload();
        public void OnAttackSwing()   => attackJuice?.OnSwing();  // on input
        public void OnAttackHit()     => attackJuice?.OnHit();    // on confirmed hit
    }
}
