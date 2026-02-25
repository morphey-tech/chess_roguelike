using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public sealed class DuelStageConfigRepository : ConfigRepository<DuelStageConfig>
    {
        [JsonProperty("content")]
        public DuelStageConfig[] Configs
        {
            get => _configs;
            set { _configs = value ?? Array.Empty<DuelStageConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<DuelStageConfig> Items => _configs;
        protected override string GetKey(DuelStageConfig item) => item.Id;

        private DuelStageConfig[] _configs = Array.Empty<DuelStageConfig>();
    }
}