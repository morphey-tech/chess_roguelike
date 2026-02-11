using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Gameplay
{
    public enum HpBarVisibilityMode
    {
        Always,
        OnHover,
        OnHoverOrSelection
    }

    public enum HpBarTeamScope
    {
        EnemiesOnly,
        AlliesOnly,
        All
    }

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

        /// <summary>
        /// Policy for when HP bars are visible.
        /// </summary>
        [JsonProperty("hpBarVisibilityMode")]
        public HpBarVisibilityMode HpBarVisibilityMode { get; set; } = HpBarVisibilityMode.Always;

        /// <summary>
        /// Which team(s) HP bar policy applies to.
        /// </summary>
        [JsonProperty("hpBarTeamScope")]
        public HpBarTeamScope HpBarTeamScope { get; set; } = HpBarTeamScope.All;
    }
}
