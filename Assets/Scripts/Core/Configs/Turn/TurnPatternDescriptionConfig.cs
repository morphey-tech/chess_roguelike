using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class TurnPatternDescriptionConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("condition_id")]
        public string ConditionId { get; set; }

        [JsonProperty("condition_params")]
        public Dictionary<string, object> ConditionParams { get; set; }

        [JsonProperty("action")]
        public ActionConfig Action { get; set; }
    }
}
