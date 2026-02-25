using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    public class SpawnPatternConfigRepository : ConfigRepository<SpawnPatternConfig>
    {
        [JsonProperty("content")]
        public SpawnPatternConfig[] Patterns
        {
            get => _patterns;
            set { _patterns = value ?? Array.Empty<SpawnPatternConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<SpawnPatternConfig> Items => _patterns;
        protected override string GetKey(SpawnPatternConfig item) => item.Id;

        private SpawnPatternConfig[] _patterns = Array.Empty<SpawnPatternConfig>();
    }
}
