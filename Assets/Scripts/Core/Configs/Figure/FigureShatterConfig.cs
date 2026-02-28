using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    /// <summary>
    /// Конфигурация разрушения фигуры на осколки при смерти.
    /// </summary>
    [Serializable]
    public sealed class FigureShatterConfig
    {
        /// <summary>
        /// ID конфига (для ссылки из GameplayConfig).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = "default";

        /// <summary>
        /// Минимальное количество осколков.
        /// </summary>
        [JsonProperty("minShards")]
        public int MinShards { get; set; } = 5;

        /// <summary>
        /// Максимальное количество осколков.
        /// </summary>
        [JsonProperty("maxShards")]
        public int MaxShards { get; set; } = 12;

        /// <summary>
        /// Минимальная сила подброса осколков вверх.
        /// </summary>
        [JsonProperty("scatterForceMin")]
        public float ScatterForceMin { get; set; } = 0.5f;

        /// <summary>
        /// Максимальная сила подброса осколков вверх.
        /// </summary>
        [JsonProperty("scatterForceMax")]
        public float ScatterForceMax { get; set; } = 1.5f;

        /// <summary>
        /// Множитель силы разброса.
        /// </summary>
        [JsonProperty("forceMultiplier")]
        public float ForceMultiplier { get; set; } = 1f;

        /// <summary>
        /// Сопротивление воздуха для осколков.
        /// </summary>
        [JsonProperty("drag")]
        public float Drag { get; set; } = 0.5f;

        /// <summary>
        /// Угловое сопротивление для осколков.
        /// </summary>
        [JsonProperty("angularDrag")]
        public float AngularDrag { get; set; } = 0.5f;

        /// <summary>
        /// Время жизни осколков в секундах (0 = не удалять, -1 = до конца уровня).
        /// </summary>
        [JsonProperty("lifetime")]
        public float Lifetime { get; set; } = -1f;

        /// <summary>
        /// Масштаб прокси-бокса относительно оригинального меша.
        /// </summary>
        [JsonProperty("proxyScale")]
        public float ProxyScale { get; set; } = 0.5f;

        /// <summary>
        /// Использовать материалы оригинального меша.
        /// </summary>
        [JsonProperty("useOriginalMaterials")]
        public bool UseOriginalMaterials { get; set; } = true;

        /// <summary>
        /// Включить двустороннюю отрисовку материалов.
        /// </summary>
        [JsonProperty("doubleSided")]
        public bool DoubleSided { get; set; } = true;

        /// <summary>
        /// Ключ запасного материала если оригинальный не найден.
        /// </summary>
        [JsonProperty("fallbackMaterialKey")]
        public string? FallbackMaterialKey { get; set; }

        /// <summary>
        /// Использовать асинхронный фрактуринг (может быть нестабильно).
        /// </summary>
        [JsonProperty("asyncFracture")]
        public bool AsyncFracture { get; set; } = false;
    }
}
