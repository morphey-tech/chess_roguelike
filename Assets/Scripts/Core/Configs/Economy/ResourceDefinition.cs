using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Economy
{
    [Serializable]
    public sealed class ResourceDefinition
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonProperty("max")]
        public int Max { get; set; } = 99999;

        [JsonProperty("meta")]
        public bool Meta { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
