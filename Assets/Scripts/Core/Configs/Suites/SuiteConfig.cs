using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Suites
{
    [Serializable]
    public class SuiteConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("figures")]
        public string Figures { get; set; }
    }
}