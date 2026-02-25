using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Suites
{
    [Serializable]
    public sealed class SuiteConfigRepository : ConfigRepository<SuiteConfig>
    {
        [JsonProperty("content")]
        public SuiteConfig[] Suites
        {
            get => _suites;
            set { _suites = value ?? Array.Empty<SuiteConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<SuiteConfig> Items => _suites;
        protected override string GetKey(SuiteConfig item) => item.Id;
        
        private SuiteConfig[] _suites = Array.Empty<SuiteConfig>();
    }
}