using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public class FigureConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("asset_key")]
        public string AssetKey { get; set; }
        
        [JsonProperty("behaviour_id")]
        public string BehaviourId { get; set; }
        
        [JsonProperty("movement_id")]
        public string MovementId { get; set; }
        
        [JsonProperty("attack_id")]
        public string AttackId { get; set; }
        
        [JsonProperty("stats_id")]
        public string StatsId { get; set; }
        
        [JsonProperty("turn_pattern_set_id")]
        public string TurnPatternSetId { get; set; }
        
        [JsonProperty("passives")]
        public string[] Passives { get; set; } = Array.Empty<string>();
    }
}
