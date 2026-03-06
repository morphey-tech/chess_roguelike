namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Standard priority values for artifact triggers.
    /// </summary>
    public static class ArtifactPriorities
    {
        /// <summary>Highest priority: Cancel death, revive (e.g. Phoenix Feather).</summary>
        public const int Critical = -100;

        /// <summary>High priority: Damage modification, dodge (e.g. Dice).</summary>
        public const int High = -50;

        /// <summary>Normal priority: Most effects (default).</summary>
        public const int Normal = 0;

        /// <summary>Low priority: Buffs, shields.</summary>
        public const int Low = 50;

        /// <summary>Lowest priority: Cleanup, visual effects.</summary>
        public const int Cleanup = 100;
    }
}