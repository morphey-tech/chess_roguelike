using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class TurnPatternsConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pattern_ids")]
        public string[] PatternIds { get; set; }
    }
}
