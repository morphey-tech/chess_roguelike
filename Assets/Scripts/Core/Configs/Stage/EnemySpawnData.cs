using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public sealed class EnemySpawnData
    {
        [JsonProperty("typeId")]
        public string TypeId { get; set; }
        
        [JsonProperty("row")]
        public int Row { get; set; }
        
        [JsonProperty("column")]
        public int Column { get; set; }
    }
}
