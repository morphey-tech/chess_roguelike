using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public class FigureDescriptionConfig
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
        public string[] Passives { get; set; } = Array.Empty<string>();
    }
}
