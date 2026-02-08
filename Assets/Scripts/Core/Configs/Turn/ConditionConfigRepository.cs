using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class ConditionConfigRepository : ConfigRepository<ConditionConfig>
    {
        private ConditionConfig[] _conditions = Array.Empty<ConditionConfig>();

        [JsonProperty("content")]
        public ConditionConfig[] Conditions
        {
            get => _conditions;
            set { _conditions = value ?? Array.Empty<ConditionConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ConditionConfig> Items => _conditions;
        protected override string GetKey(ConditionConfig item) => item.Id;
    }
}