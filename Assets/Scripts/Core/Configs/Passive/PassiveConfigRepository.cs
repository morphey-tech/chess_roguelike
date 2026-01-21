using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Passive
{
    [Serializable]
    public sealed class PassiveConfigRepository
    {
        [JsonProperty("content")]
        public PassiveConfig[] Passives { get; set; } = Array.Empty<PassiveConfig>();
    }
}
