using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Gameplay
{
    /// <summary>
    /// Global gameplay settings.
    /// </summary>
    [Serializable]
    public class GameplayConfig
    {
        /// <summary>
        /// If true, enemy figures are hidden during the placement phase.
        /// </summary>
        [JsonProperty("hideEnemiesDuringPrepare")]
        public bool HideEnemiesDuringPrepare { get; set; } = true;
        
        /// <summary>
        /// Duration of figure spawn animation in seconds.
        /// </summary>
        [JsonProperty("figureSpawnDuration")]
        public float FigureSpawnDuration { get; set; } = 0.3f;
        
        /// <summary>
        /// Delay between slot spawns in prepare zone (seconds).
        /// </summary>
        [JsonProperty("prepareSlotSpawnDelay")]
        public float PrepareSlotSpawnDelay { get; set; } = 0.05f;
    }
}
