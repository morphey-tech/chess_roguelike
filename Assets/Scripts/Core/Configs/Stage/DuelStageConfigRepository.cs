using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public sealed class DuelStageConfigRepository
    {
        [JsonProperty("content")]
        public DuelStageConfig[] Configs { get; set; }
    }
}