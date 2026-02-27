using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    /// <summary>
    /// Репозиторий для хранения и доступа к расширенной информации о фигурах.
    /// </summary>
    [Serializable]
    public sealed class FigureInfoConfigRepository : ConfigRepository<FigureInfoConfig>
    {
        [JsonProperty("content")]
        public FigureInfoConfig[] Figures
        {
            get => _figures;
            set { _figures = value ?? Array.Empty<FigureInfoConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<FigureInfoConfig> Items => _figures;
        protected override string GetKey(FigureInfoConfig item) => item.Id;

        private FigureInfoConfig[] _figures = Array.Empty<FigureInfoConfig>();
    }
}
