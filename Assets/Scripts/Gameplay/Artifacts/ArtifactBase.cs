using Project.Core.Core.Configs.Artifacts;

namespace Project.Gameplay.Gameplay.Artifacts
{
    public abstract class ArtifactBase : IArtifact
    {
        protected ArtifactConfig Config { get; }

        public string ConfigId => Config.Id;

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
