using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public class StageConfigRepository
    {
        [JsonProperty("content")]
        public StageConfig[] Stages { get; set; }
    }
}