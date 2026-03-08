using MessagePipe;

namespace Project.Gameplay.Gameplay.Artifacts.Messages
{
    /// <summary>
    /// Published when an artifact is added.
    /// </summary>
    public readonly struct ArtifactAddedMessage
    {
        public ArtifactInstance Instance { get; }

        public ArtifactAddedMessage(ArtifactInstance instance)
        {
            Instance = instance;
        }
    }

    /// <summary>
    /// Published when an artifact is removed.
    /// </summary>
    public readonly struct ArtifactRemovedMessage
    {
        public string ConfigId { get; }

        public ArtifactRemovedMessage(string configId)
        {
            ConfigId = configId;
        }
    }

    /// <summary>
    /// Published when all artifacts are cleared.
    /// </summary>
    public readonly struct ArtifactsClearedMessage
    {
    }
}
