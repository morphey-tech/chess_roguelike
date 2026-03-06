using Project.Core.Core.Configs.Artifacts;
using Project.Gameplay.Gameplay.Artifacts.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    public sealed class ReviveArtifact : ArtifactBase, IOnAllyDeath
    {
        private readonly float _percent;
        private readonly int _maxTriggers;
        private int _triggerCount;

        public ReviveArtifact(ArtifactConfig config) : base(config)
        {
            _percent = config.Effect.Value;
            _maxTriggers = config.Effect.MaxTriggers > 0 ? config.Effect.MaxTriggers : 1;
            _triggerCount = 0;
        }

        public override void OnAcquired(ArtifactContext context)
        {
            _triggerCount = 0;
        }

        public void OnAllyDeath(DeathContext context)
        {
            if (_triggerCount >= _maxTriggers)
            {
                return;
            }

            // Revive fallen ally with _percent HP
            // Implementation depends on figure system
            _triggerCount++;
        }
    }
}