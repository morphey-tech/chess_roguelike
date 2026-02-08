using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public class FigureConfigRepository : ConfigRepository<FigureConfig>
    {
        private FigureConfig[] _figures = Array.Empty<FigureConfig>();

        [JsonProperty("content")]
        public FigureConfig[] Figures
        {
            get => _figures;
            set { _figures = value ?? Array.Empty<FigureConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<FigureConfig> Items => _figures;

        protected override string GetKey(FigureConfig item) => item.Id;
    }
}