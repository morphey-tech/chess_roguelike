using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Base interface for all artifact effects.
    /// Now implements ITrigger for unified execution.
    /// </summary>
    public interface IArtifact : ITrigger
    {
        /// <summary>Artifact config ID.</summary>
        string ConfigId { get; }

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
