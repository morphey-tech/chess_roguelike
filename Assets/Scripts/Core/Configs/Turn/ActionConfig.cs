using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class ActionConfig
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("strategy")]
        public string Strategy { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sub_actions")]
        public ActionConfig[] SubActions { get; set; }
    }
}
