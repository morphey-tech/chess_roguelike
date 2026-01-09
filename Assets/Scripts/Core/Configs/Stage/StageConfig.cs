using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public class StageConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("board_id")]
        public string BoardId { get; set; }
    }
}