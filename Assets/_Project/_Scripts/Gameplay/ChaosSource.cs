namespace ThePromisedRun.Gameplay {
    /// <summary>
    /// Identifies the source of chaos added to the ChaosMeter.
    /// Used for debugging, balancing, and future analytics.
    /// </summary>
    public enum ChaosSource {
        Jump                = 0,
        Attack              = 1,
        TrapHit             = 2,
        EnemyHit            = 3,
        SystemInterference  = 4,  // Helper System popup spawns, UI spam, player fail due to popup
        Manual              = 99
    }
}
