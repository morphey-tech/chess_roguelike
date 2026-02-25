using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stats
{
    [Serializable]
    public sealed class StatsConfigRepository : ConfigRepository<StatsConfig>
    {
        [JsonProperty("content")]
        public StatsConfig[] Configs
        {
            get => _configs;
            set { _configs = value ?? Array.Empty<StatsConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<StatsConfig> Items => _configs;
        protected override string GetKey(StatsConfig item) => item.Id;

        private StatsConfig[] _configs = Array.Empty<StatsConfig>();
    }
}