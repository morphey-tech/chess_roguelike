using System;
using Newtonsoft.Json;
using Project.Gameplay.Gameplay.Stage;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public class StageConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public StageType Type { get; set; }
        
        [JsonProperty("type_config")]
        public string TypeConfigId { get; set; }
        
        [JsonProperty("board_id")]
        public string BoardId { get; set; }
        
    }
}