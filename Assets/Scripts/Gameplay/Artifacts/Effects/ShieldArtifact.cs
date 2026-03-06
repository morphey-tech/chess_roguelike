using Project.Core.Core.Configs.Artifacts;
using Project.Gameplay.Gameplay.Artifacts.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    /// <summary>
    /// Shield grant artifact (e.g. +3 shield at battle start).
    /// </summary>
    public sealed class ShieldArtifact : ArtifactBase, IOnBattleStart
    {
        private readonly float _value;

        public ShieldArtifact(ArtifactConfig config) : base(config)
        {
            _value = config.Effect.Value;
        }

        public void OnBattleStart(BattleContext context)
        {
            // Apply shield to target
            // Implementation depends on shield system
        }
    }
}