using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public sealed class StageSequenceConfig
    {
        [JsonProperty("stages")]
        public string[] Stages { get; set; } = Array.Empty<string>();
    }
}
