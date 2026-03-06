using Project.Core.Core.Configs.Artifacts;
using Project.Gameplay.Gameplay.Artifacts.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    public sealed class HealArtifact : ArtifactBase, IOnUnitKill
    {
        private readonly string _target; // "killer", "king", "lowest_hp"
        private readonly float _value;

        public HealArtifact(ArtifactConfig config) : base(config)
        {
            _target = config.Effect.Target;
            _value = config.Effect.Value;
        }

        public void OnUnitKill(KillContext context)
        {
            // Apply healing to target
            // Implementation depends on health system
        }
    }
}