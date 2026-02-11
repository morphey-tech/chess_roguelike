using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Loot
{
    [Serializable]
    public sealed class LootEntryConfig
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "nothing";

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("min")]
        public int Min { get; set; }

        [JsonProperty("max")]
        public int Max { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; } = 1;
    }
}
