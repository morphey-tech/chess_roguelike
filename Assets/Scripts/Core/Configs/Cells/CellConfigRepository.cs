using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Cells
{
    [Serializable]
    public class CellConfigRepository : ConfigRepository<CellConfig>
    {
        private CellConfig[] _cells = Array.Empty<CellConfig>();

        [JsonProperty("content")]
        public CellConfig[] Cells
        {
            get => _cells;
            set { _cells = value ?? Array.Empty<CellConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<CellConfig> Items => _cells;
        protected override string GetKey(CellConfig item) => item.Alias;
    }
}