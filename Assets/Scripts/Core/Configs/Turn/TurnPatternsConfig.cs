using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class TurnPatternsConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pattern_ids")]
        public string[] PatternIds { get; set; }
    }

    public class TurnPatternsConfigRepository : ConfigRepository<TurnPatternsConfig>
    {
        private TurnPatternsConfig[] _patterns = Array.Empty<TurnPatternsConfig>();

        [JsonProperty("content")]
        public TurnPatternsConfig[] Patterns
        {
            get => _patterns;
            set { _patterns = value ?? Array.Empty<TurnPatternsConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<TurnPatternsConfig> Items => _patterns;
        protected override string GetKey(TurnPatternsConfig item) => item.Id;
    }
}
