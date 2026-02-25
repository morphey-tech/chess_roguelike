using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public class TurnPatternsConfigRepository : ConfigRepository<TurnPatternsConfig>
    {
        [JsonProperty("content")]
        public TurnPatternsConfig[] Patterns
        {
            get => _patterns;
            set { _patterns = value ?? Array.Empty<TurnPatternsConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<TurnPatternsConfig> Items => _patterns;
        protected override string GetKey(TurnPatternsConfig item) => item.Id;
        
        private TurnPatternsConfig[] _patterns = Array.Empty<TurnPatternsConfig>();
    }
}