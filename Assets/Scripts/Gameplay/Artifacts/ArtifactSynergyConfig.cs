using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Configuration for artifact synergy.
    /// </summary>
    public class ArtifactSynergyConfig
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> RequiredArtifactIds { get; set; } = new();
        public int RequiredCount { get; set; } = 2;
        public string EffectId { get; set; } = "";
    }
}
