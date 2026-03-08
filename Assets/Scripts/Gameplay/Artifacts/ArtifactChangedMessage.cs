namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Message published when artifact list changes.
    /// </summary>
    public readonly struct ArtifactChangedMessage
    {
        public int OwnerId { get; }
        public string? ArtifactId { get; }
        public ArtifactChangeType ChangeType { get; }

        public ArtifactChangedMessage(int ownerId, string? artifactId, ArtifactChangeType changeType)
        {
            OwnerId = ownerId;
            ArtifactId = artifactId;
            ChangeType = changeType;
        }
    }

    public enum ArtifactChangeType
    {
        Acquired,
        Removed,
        Equipped,
        Unequipped,
    }
}
