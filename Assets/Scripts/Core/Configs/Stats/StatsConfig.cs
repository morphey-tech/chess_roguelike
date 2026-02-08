using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stats
{
    [Serializable]
    public sealed class StatsConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("max_hp")]
        public int MaxHp { get; set; }
        
        [JsonProperty("attacks")]
        public AttackConfig[] Attacks { get; set; } = Array.Empty<AttackConfig>();
    }
}