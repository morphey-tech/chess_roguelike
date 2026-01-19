using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public class FigureConfigRepository
    {
        [JsonProperty("content")]
        public FigureConfig[] Figures { get; set; }

        public FigureConfig GetBy(string id)
        {
            return Array.Find(Figures, f => f.Id == id);
        }
    }
}