using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Loot
{
    [Serializable]
    public sealed class LootTableConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("rolls")]
        public int Rolls { get; set; } = 1;

        [JsonProperty("entries")]
        public LootEntryConfig[] Entries { get; set; } = Array.Empty<LootEntryConfig>();
    }
}
