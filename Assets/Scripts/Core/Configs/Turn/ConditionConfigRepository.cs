using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class ConditionConfigRepository : ConfigRepository<ConditionConfig>
    {
        [JsonProperty("content")]
        public ConditionConfig[] Conditions
        {
            get => _conditions;
            set { _conditions = value ?? Array.Empty<ConditionConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ConditionConfig> Items => _conditions;
        protected override string GetKey(ConditionConfig item) => item.Id;
        
        private ConditionConfig[] _conditions = Array.Empty<ConditionConfig>();
    }
}