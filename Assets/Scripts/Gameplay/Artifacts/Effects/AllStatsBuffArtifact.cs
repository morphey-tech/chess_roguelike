using Project.Core.Core.Configs.Artifacts;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    /// <summary>
    /// All stats buff artifact (e.g. +1 to all stats).
    /// </summary>
    public sealed class AllStatsBuffArtifact : ArtifactBase
    {
        private readonly float _value;
        private readonly ArtifactBuffDuration _duration;

        public AllStatsBuffArtifact(ArtifactConfig config) : base(config)
        {
            _value = config.Effect.Value;
            _duration = config.Effect.ParseDuration();
        }

        public override void OnAcquired(ArtifactContext context)
        {
            // Apply all stats buff
            // Implementation depends on stat system
        }

        public override void OnRemoved(ArtifactContext context)
        {
            // Remove buff if not permanent
            if (_duration != ArtifactBuffDuration.Permanent)
            {
                // Remove buff
            }
        }
    }
}