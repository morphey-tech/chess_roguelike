using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Cells
{
    [Serializable]
    public class CellConfigRepository : ConfigRepository<CellConfig>
    {
        [JsonProperty("content")]
        public CellConfig[] Cells
        {
            get => _cells;
            set { _cells = value ?? Array.Empty<CellConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<CellConfig> Items => _cells;
        protected override string GetKey(CellConfig item) => item.Alias;

        private CellConfig[] _cells = Array.Empty<CellConfig>();
    }
}