using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stats
{
    [Serializable]
    public sealed class AttackConfig
    {
        [JsonProperty("type")]
        public AttackType Type { get; set; }

        [JsonProperty("damage")]
        public int Damage { get; set; }

        [JsonProperty("range")]
        public int Range { get; set; }

        [JsonProperty("targeting")]
        public TargetingType Targeting { get; set; }
    }
}
