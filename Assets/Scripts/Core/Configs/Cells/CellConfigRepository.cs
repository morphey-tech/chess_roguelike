using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Cells
{
    [Serializable]
    public class CellConfigRepository
    {
        [JsonProperty("content")]
        public List<CellConfig> Cells { get; set; }
    }
}