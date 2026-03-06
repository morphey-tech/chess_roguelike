namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Runtime instance of an artifact with stack count.
    /// </summary>
    public sealed class ArtifactInstance
    {
        /// <summary>
        /// Unique instance ID (for tracking individual artifacts).
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The artifact definition.
        /// </summary>
        public IArtifact Artifact { get; }

        /// <summary>
        /// Config ID for save/load.
        /// </summary>
        public string ConfigId => Artifact.ConfigId;

        /// <summary>
        /// Stack count for stackable artifacts.
        /// </summary>
        public int Stack { get; set; } = 1;

        /// <summary>
        /// Trigger priority (cached from artifact).
        /// </summary>
        public int Priority => Artifact.Priority;

        public ArtifactInstance(IArtifact artifact, string id)
        {
            Artifact = artifact;
            Id = id;
        }

        /// <summary>
        /// Try to stack with another instance of the same artifact.
        /// Returns true if stacked, false if different artifact.
        /// </summary>
        public bool TryStackWith(ArtifactInstance other)
        {
            if (other == null || other.ConfigId != ConfigId)
            {
                return false;
            }

            Stack += other.Stack;
            return true;
        }

        /// <summary>
        /// Get effective value multiplied by stack count.
        /// </summary>
        public int GetStackedValue(int baseValue)
        {
            return baseValue * Stack;
        }

        /// <summary>
        /// Get effective float value multiplied by stack count.
        /// </summary>
        public float GetStackedValue(float baseValue)
        {
            return baseValue * Stack;
        }
    }
}
