using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public class FigureDescriptionConfigRepository
    {
        [JsonProperty("content")]
        public FigureDescriptionConfig[] Descriptions { get; set; }
    }
}