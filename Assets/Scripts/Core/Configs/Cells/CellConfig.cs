using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Cells
{
    [Serializable]
    public class CellConfig
    {
        [JsonProperty("board_data_alias")]
        public string Alias { get; set; }
        
        [JsonProperty("asset_key")]
        public string AssetKey { get; set; }
    }
}