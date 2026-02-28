using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    /// <summary>
    /// Репозиторий конфигов разрушения фигур при смерти.
    /// </summary>
    [Serializable]
    public class FigureShatterConfigRepository : ConfigRepository<FigureShatterConfig>
    {
        [JsonProperty("content")]
        public FigureShatterConfig[] Configs
        {
            get => _configs;
            set { _configs = value ?? Array.Empty<FigureShatterConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<FigureShatterConfig> Items => _configs;
        protected override string GetKey(FigureShatterConfig item) => item.Id;

        private FigureShatterConfig[] _configs = Array.Empty<FigureShatterConfig>();
    }
}
