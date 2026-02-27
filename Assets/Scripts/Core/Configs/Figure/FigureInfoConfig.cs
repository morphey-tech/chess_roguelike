using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    /// <summary>
    /// Расширенная информация о фигуре для отображения в UI.
    /// </summary>
    [Serializable]
    public sealed class FigureInfoConfig
    {
        /// <summary>
        /// ID фигуры (связь с FigureDescriptionConfig).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Отображаемое название фигуры.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Краткое описание фигуры.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Ключ спрайта/иконки фигуры для UI.
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Ключ префаба иконки для отображения в окне информации.
        /// </summary>
        [JsonProperty("icon_prefab")]
        public string IconPrefab { get; set; }
    }
}
