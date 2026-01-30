using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public sealed class DuelStageConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("spawn_pattern_id")]
        public string SpawnPatternId { get; set; }
        
        [JsonProperty("enemies")]
        public EnemySpawnData[] Enemies { get; set; }
    }
}
