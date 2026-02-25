using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public class StageConfigRepository : ConfigRepository<StageConfig>
    {
        [JsonProperty("content")]
        public StageConfig[] Stages
        {
            get => _stages;
            set { _stages = value ?? Array.Empty<StageConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<StageConfig> Items => _stages;
        protected override string GetKey(StageConfig item) => item.Id;

        private StageConfig[] _stages = Array.Empty<StageConfig>();
    }
}