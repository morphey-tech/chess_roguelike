using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Loot
{
    [Serializable]
    public sealed class LootTableRepository : ConfigRepository<LootTableConfig>
    {
        private LootTableConfig[] _tables = Array.Empty<LootTableConfig>();

        [JsonProperty("tables")]
        public LootTableConfig[] Tables
        {
            get => _tables;
            set { _tables = value ?? Array.Empty<LootTableConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<LootTableConfig>? Items => _tables;
        protected override string GetKey(LootTableConfig item) => item.Id;
    }
}
