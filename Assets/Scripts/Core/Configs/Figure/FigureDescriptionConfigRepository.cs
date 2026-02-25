using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    [Serializable]
    public class FigureDescriptionConfigRepository : ConfigRepository<FigureDescriptionConfig>
    {
        [JsonProperty("content")]
        public FigureDescriptionConfig[] Descriptions
        {
            get => _descriptions;
            set { _descriptions = value ?? Array.Empty<FigureDescriptionConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<FigureDescriptionConfig> Items => _descriptions;
        protected override string GetKey(FigureDescriptionConfig item) => item.Id;

        private FigureDescriptionConfig[] _descriptions = Array.Empty<FigureDescriptionConfig>();
    }
}