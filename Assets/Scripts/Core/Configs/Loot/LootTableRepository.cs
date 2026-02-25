using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Loot
{
    [Serializable]
    public sealed class LootTableRepository : ConfigRepository<LootTableConfig>
    {
        [JsonProperty("tables")]
        public LootTableConfig[] Tables
        {
            get => _tables;
            set { _tables = value ?? Array.Empty<LootTableConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<LootTableConfig>? Items => _tables;
        protected override string GetKey(LootTableConfig item) => item.Id;

        private LootTableConfig[] _tables = Array.Empty<LootTableConfig>();
    }
}
