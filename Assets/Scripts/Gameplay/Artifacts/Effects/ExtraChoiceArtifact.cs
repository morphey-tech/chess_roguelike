using Project.Core.Core.Configs.Artifacts;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    public sealed class ExtraChoiceArtifact : ArtifactBase
    {
        private readonly float _value;

        public ExtraChoiceArtifact(ArtifactConfig config) : base(config)
        {
            _value = config.Effect.Value;
        }

        // This artifact is queried by loot system, not triggered
    }
}