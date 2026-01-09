using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Run
{
    [Serializable]
    public class RunConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("stages")]
        public string[] Stages { get; set; }
    }
}