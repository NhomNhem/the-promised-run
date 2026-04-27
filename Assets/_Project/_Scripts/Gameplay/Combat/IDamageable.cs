namespace ThePromisedRun.Gameplay.Combat {
    /// <summary>
    /// Any entity that can receive damage implements this.
    /// </summary>
    public interface IDamageable {
        void TakeDamage(float amount, DamageInfo info);
        bool IsAlive { get; }
    }
}
