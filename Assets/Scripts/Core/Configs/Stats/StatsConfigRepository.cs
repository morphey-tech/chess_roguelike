using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stats
{
    [Serializable]
    public sealed class StatsConfigRepository
    {
        [JsonProperty("content")]
        public StatsConfig[] Configs { get; set; }
    }
}