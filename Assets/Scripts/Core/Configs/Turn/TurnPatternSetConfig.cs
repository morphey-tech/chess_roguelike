using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class TurnPatternSetConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pattern_ids")]
        public string[] PatternIds { get; set; }
    }

    public sealed class TurnPatternSetConfigRepository
    {
        [JsonProperty("sets")]
        public TurnPatternSetConfig[] Sets { get; set; }
    }
}
