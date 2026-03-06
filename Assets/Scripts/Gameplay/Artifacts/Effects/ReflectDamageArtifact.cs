using Project.Core.Core.Configs.Artifacts;
using Project.Gameplay.Gameplay.Artifacts.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    /// <summary>
    /// Damage reflection artifact (e.g. deal 2 back when hit).
    /// </summary>
    public sealed class ReflectDamageArtifact : ArtifactBase, IOnDamageReceived
    {
        private readonly float _value;

        public ReflectDamageArtifact(ArtifactConfig config) : base(config)
        {
            _value = config.Effect.Value;
        }

        public void OnDamageReceived(DamageContext context)
        {
            // Reflect damage back to attacker
            // Implementation depends on combat system
        }
    }
}