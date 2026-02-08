using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Stage
{
    public class SpawnPatternConfigRepository : ConfigRepository<SpawnPatternConfig>
    {
        private SpawnPatternConfig[] _patterns = Array.Empty<SpawnPatternConfig>();

        [JsonProperty("content")]
        public SpawnPatternConfig[] Patterns
        {
            get => _patterns;
            set { _patterns = value ?? Array.Empty<SpawnPatternConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<SpawnPatternConfig> Items => _patterns;
        protected override string GetKey(SpawnPatternConfig item) => item.Id;
    }
}
