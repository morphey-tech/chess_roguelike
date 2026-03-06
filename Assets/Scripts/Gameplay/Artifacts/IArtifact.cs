using Project.Core.Core.Configs.Artifacts;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Base interface for all artifact effects.
    /// </summary>
    public interface IArtifact
    {
        /// <summary>Artifact config ID.</summary>
        string ConfigId { get; }

        /// <summary>
        /// Priority for trigger execution order.
        /// Lower values execute first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Tags for categorization and synergies.
        /// </summary>
        ArtifactTag Tags { get; }

        /// <summary>Called when artifact is first acquired.</summary>
        void OnAcquired(ArtifactContext context);

        /// <summary>Called when artifact is removed/lost.</summary>
        void OnRemoved(ArtifactContext context);
    }
}
