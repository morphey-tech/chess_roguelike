using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Base class for artifact implementations.
    /// Implements ITrigger for unified execution.
    /// </summary>
    public abstract class ArtifactBase : IArtifact
    {
        protected ArtifactConfig Config { get; }

        public string ConfigId => Config.Id;

        /// <summary>
        /// Priority for trigger execution. Override in derived classes.
        /// </summary>
        public virtual int Priority => TriggerPriorities.Normal;

        /// <summary>
        /// Group within phase. Override for specific group execution.
        /// </summary>
        public virtual TriggerGroup Group => TriggerGroup.Default;

        /// <summary>
        /// Phase for this trigger. Override for specific phase execution.
        /// </summary>
        public virtual TriggerPhase Phase => TriggerPhase.Default;

        /// <summary>
        /// Tags from config. Override in derived classes if needed.
        /// </summary>
        public virtual ArtifactTag Tags => Config.ParseTags();

        protected ArtifactBase(ArtifactConfig config)
        {
            Config = config;
        }

        public virtual bool Matches(TriggerContext context)
        {
            // Default: match all contexts (passive artifacts)
            return true;
        }

        public virtual TriggerResult Execute(TriggerContext context)
        {
            // Default: do nothing (passive artifact)
            return TriggerResult.Continue;
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
