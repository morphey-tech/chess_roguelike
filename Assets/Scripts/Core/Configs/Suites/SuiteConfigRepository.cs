using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Suites
{
    [Serializable]
    public sealed class SuiteConfigRepository : ConfigRepository<SuiteConfig>
    {
        private SuiteConfig[] _suites = Array.Empty<SuiteConfig>();

        [JsonProperty("content")]
        public SuiteConfig[] Suites
        {
            get => _suites;
            set { _suites = value ?? Array.Empty<SuiteConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<SuiteConfig> Items => _suites;
        protected override string GetKey(SuiteConfig item) => item.Id;
    }
}