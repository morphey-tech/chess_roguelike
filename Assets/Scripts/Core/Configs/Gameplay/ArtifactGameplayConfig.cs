using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Gameplay
{
    /// <summary>
    /// Artifact system configuration.
    /// </summary>
    [Serializable]
    public sealed class ArtifactGameplayConfig
    {
        [JsonProperty("maxArtifacts")]
        public int MaxArtifacts { get; set; } = 8;
    }
}
