using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class TurnPatternDescriptionConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("condition_id")]
        public string ConditionId { get; set; }

        [JsonProperty("condition_params")]
        public Dictionary<string, object> ConditionParams { get; set; }

        /// <summary>
        /// Action config.
        /// </summary>
        [JsonProperty("action")]
        public Core.Configs.Turn.ActionConfig Action { get; set; }
    }

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
