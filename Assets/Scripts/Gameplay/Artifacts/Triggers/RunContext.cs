namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Context for run-level artifact effects (non-combat).
    /// Used for artifacts that affect meta-progression, rewards, etc.
    /// </summary>
    public sealed class RunContext
    {
        /// <summary>
        /// Current run ID.
        /// </summary>
        public string RunId { get; set; } = string.Empty;

        /// <summary>
        /// Current stage ID.
        /// </summary>
        public string StageId { get; set; } = string.Empty;

        /// <summary>
        /// Turn count in current run.
        /// </summary>
        public int TurnCount { get; set; }

        /// <summary>
        /// Number of enemies killed in current run.
        /// </summary>
        public int KillsCount { get; set; }

        /// <summary>
        /// Current crowns (run currency).
        /// </summary>
        public int Crowns { get; set; }

        /// <summary>
        /// Current scrolls (meta currency).
        /// </summary>
        public int Scrolls { get; set; }

        /// <summary>
        /// Number of reward choices available.
        /// Modified by artifacts like Ancient Scroll.
        /// </summary>
        public int RewardChoices { get; set; } = 3;

        /// <summary>
        /// Create empty run context.
        /// </summary>
        public static RunContext Create()
        {
            return new RunContext();
        }
    }
}
