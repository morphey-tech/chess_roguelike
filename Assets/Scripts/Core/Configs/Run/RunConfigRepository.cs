using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Run
{
    [Serializable]
    public class RunConfigRepository
    {
        [JsonProperty("content")]
        public RunConfig[] Runs { get; set; }
    }
}