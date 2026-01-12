using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public class FigureConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("asset_key")]
        public string AssetKey { get; set; }
        
        [JsonProperty("behaviour_id")]
        public string BehaviourId { get; set; }
    }
}