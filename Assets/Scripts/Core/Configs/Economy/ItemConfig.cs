using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Economy
{
    [Serializable]
    public sealed class ItemConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; } = ItemCategories.Artifact;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("max_stack")]
        public int MaxStack { get; set; } = 1;

        [JsonProperty("lifetime")]
        public string Lifetime { get; set; } = "run";

        [JsonProperty("passives")]
        public string[] Passives { get; set; } = Array.Empty<string>();

        [JsonProperty("params")]
        public Dictionary<string, float> Params { get; set; } = new();

        public ItemLifetime ParseLifetime()
        {
            return Lifetime?.ToLowerInvariant() switch
            {
                "meta" => ItemLifetime.Meta,
                "temporary" => ItemLifetime.Temporary,
                _ => ItemLifetime.Run
            };
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return Params != null && Params.TryGetValue(key, out float v) ? v : defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0) => (int)GetFloat(key, defaultValue);
        public bool GetBool(string key, bool defaultValue = false) => GetFloat(key, defaultValue ? 1f : 0f) != 0f;
    }
}
