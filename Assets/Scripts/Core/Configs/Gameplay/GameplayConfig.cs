using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Gameplay
{
    [Serializable]
    public sealed class GameplayConfig
    {
        /// <summary>
        /// If true, enemy figures are hidden during the placement phase.
        /// </summary>
        [JsonProperty("hideEnemiesDuringPrepare")]
        public bool HideEnemiesDuringPrepare { get; set; } = true;

        /// <summary>
        /// If true, HP bars are hidden during the preparation phase.
        /// </summary>
        [JsonProperty("hideHpBarsDuringPrepare")]
        public bool HideHpBarsDuringPrepare { get; set; } = true;

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

        /// <summary>
        /// Policy for when HP bars are visible for allies (Player team).
        /// </summary>
        [JsonProperty("hpBarVisibilityModeAllies")]
        public HpBarVisibilityMode HpBarVisibilityModeAllies { get; set; } = HpBarVisibilityMode.Always;

        /// <summary>
        /// Policy for when HP bars are visible for enemies (Enemy team).
        /// </summary>
        [JsonProperty("hpBarVisibilityModeEnemies")]
        public HpBarVisibilityMode HpBarVisibilityModeEnemies { get; set; } = HpBarVisibilityMode.OnHoverOrSelection;
    }
}
