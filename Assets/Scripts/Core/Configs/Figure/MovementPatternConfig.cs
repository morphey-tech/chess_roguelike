using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Figure
{
    /// <summary>
    /// Movement pattern configuration for PatternMovement strategy.
    /// </summary>
    [Serializable]
    public class MovementPatternConfig
    {
        /// <summary>Unique identifier for this movement pattern.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Direction type: "straight", "diagonal", "cross", "all".
        /// straight: forward/backward/left/right
        /// diagonal: 4 diagonal directions
        /// cross: same as straight
        /// all: 8 directions (straight + diagonal)
        /// </summary>
        [JsonProperty("directions")]
        public string Directions { get; set; } = "straight";

        /// <summary>Minimum movement distance (default: 1).</summary>
        [JsonProperty("min_distance")]
        public int MinDistance { get; set; } = 1;

        /// <summary>Maximum movement distance (default: 1).</summary>
        [JsonProperty("max_distance")]
        public int MaxDistance { get; set; } = 1;

        /// <summary>If true, can jump over occupied cells (default: false).</summary>
        [JsonProperty("jump_over")]
        public bool JumpOver { get; set; } = false;

        /// <summary>Extra straight distance for special movements like slippery (default: 0).</summary>
        [JsonProperty("extra_straight_distance")]
        public int ExtraStraightDistance { get; set; } = 0;
    }
}
