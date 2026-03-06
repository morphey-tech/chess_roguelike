using Project.Core.Core.Configs.Artifacts;
using Project.Gameplay.Gameplay.Artifacts.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    public sealed class StatBuffArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly string _stat;
        private readonly float _value;
        private readonly ArtifactBuffDuration _duration;
        private readonly bool _stackable;

        public StatBuffArtifact(ArtifactConfig config) : base(config)
        {
            _stat = config.Effect.Stat;
            _value = config.Effect.Value;
            _duration = config.Effect.ParseDuration();
            _stackable = config.Effect.Stackable;
        }

        public void OnBattleStart(BattleContext context)
        {
            // Apply stat buff to target
            // Implementation depends on stat system
        }
    }
}
