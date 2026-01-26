using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class StepConfig
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("strategy")]
        public string Strategy { get; set; }

        [JsonProperty("steps")]
        public StepConfig[] Steps { get; set; }
    }
}
