using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Suites
{
    [Serializable]
    public sealed class SuiteConfigRepository
    {
        [JsonProperty("content")]
        public SuiteConfig[] Suites { get; set; }
    }
}