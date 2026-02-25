using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public class TurnPatternDescriptionConfigRepository : ConfigRepository<TurnPatternDescriptionConfig>
    {
        private TurnPatternDescriptionConfig[] _descriptions = Array.Empty<TurnPatternDescriptionConfig>();

        [JsonProperty("content")]
        public TurnPatternDescriptionConfig[] Descriptions
        {
            get => _descriptions;
            set { _descriptions = value ?? Array.Empty<TurnPatternDescriptionConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<TurnPatternDescriptionConfig> Items => _descriptions;
        protected override string GetKey(TurnPatternDescriptionConfig item) => item.Id;
    }
}