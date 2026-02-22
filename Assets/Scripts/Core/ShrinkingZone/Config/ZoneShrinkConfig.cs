using System;
using Newtonsoft.Json;

namespace Project.Core.Core.ShrinkingZone.Config
{
    /// <summary>
    /// Конфигурация shrinking zone
    /// </summary>
    [Serializable]
    public class ZoneShrinkConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("min_turn")]
        public int MinTurn { get; set; } = 3;

        [JsonProperty("max_turn")]
        public int MaxTurn { get; set; } = 6;

        [JsonProperty("shrink_interval")]
        public int ShrinkInterval { get; set; } = 2;

        [JsonProperty("zone_damage_flat")]
        public int ZoneDamageFlat { get; set; } = 5;

        [JsonProperty("zone_damage_percent")]
        public float ZoneDamagePercent { get; set; } = 0.1f;

        [JsonProperty("safe_zone_min_size")]
        public int SafeZoneMinSize { get; set; } = 2;

        [JsonProperty("board_size")]
        public int BoardSize { get; set; } = 8;
    }
}
