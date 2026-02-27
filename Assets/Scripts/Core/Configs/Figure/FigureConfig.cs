using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    public class FigureConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("alias")]
        public string Alias { get; set; }
        
        [JsonProperty("asset_key")]
        public string AssetKey { get; set; }
        
        [JsonProperty("description_id")]
        public string DescriptionId { get; set; }
        
        [JsonProperty("info_id")]
        public string InfoId { get; set; }
    }
}
