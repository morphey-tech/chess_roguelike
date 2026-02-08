using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public class StageConfigRepository : ConfigRepository<StageConfig>
    {
        private StageConfig[] _stages = Array.Empty<StageConfig>();

        [JsonProperty("content")]
        public StageConfig[] Stages
        {
            get => _stages;
            set { _stages = value ?? Array.Empty<StageConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<StageConfig> Items => _stages;
        protected override string GetKey(StageConfig item) => item.Id;
    }
}