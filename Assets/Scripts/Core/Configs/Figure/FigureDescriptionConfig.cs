using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public sealed class FigureDescriptionConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("movement_id")]
        public string MovementId { get; set; }
        
        [JsonProperty("attack_id")]
        public string AttackId { get; set; }
        
        [JsonProperty("stats_id")]
        public string StatsId { get; set; }
        
        [JsonProperty("turn_patterns_id")]
        public string TurnPatternsId { get; set; }
        
        [JsonProperty("passives")]
        public string[]? Passives { get; set; } = Array.Empty<string>();
        
        [JsonProperty("load")]
        public int Load { get; set; } = 1;

        [JsonProperty("loot_table_id")]
        public string LootTableId { get; set; } = string.Empty;

        [JsonProperty("movement_pattern")]
        public MovementPatternConfig? MovementPattern { get; set; }
    }
}
