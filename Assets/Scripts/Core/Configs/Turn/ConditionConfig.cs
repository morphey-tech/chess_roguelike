using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class ConditionConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, object> Params { get; set; }
    }
}
