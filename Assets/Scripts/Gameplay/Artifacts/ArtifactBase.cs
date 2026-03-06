using Project.Core.Core.Configs.Artifacts;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Base class for artifact implementations.
    /// </summary>
    public abstract class ArtifactBase : IArtifact
    {
        protected ArtifactConfig Config { get; }

        public string ConfigId => Config.Id;

        /// <summary>
        /// Priority for trigger execution. Override in derived classes.
        /// </summary>
        public virtual int Priority => ArtifactPriorities.Normal;

        /// <summary>
        /// Tags from config. Override in derived classes if needed.
        /// </summary>
        public virtual ArtifactTag Tags => Config.ParseTags();

        protected ArtifactBase(ArtifactConfig config)
        {
            Config = config;
        }

        public virtual void OnAcquired(ArtifactContext context)
        {
            // Optional setup when artifact is acquired
        }

        public virtual void OnRemoved(ArtifactContext context)
        {
            // Optional cleanup when artifact is removed
        }
    }
}
