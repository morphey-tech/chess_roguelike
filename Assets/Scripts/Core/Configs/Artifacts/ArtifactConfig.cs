using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Artifacts
{
    /// <summary>
    /// Effect configuration for an artifact.
    /// </summary>
    [Serializable]
    public sealed class ArtifactEffectConfig
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "none";

        [JsonProperty("stat")]
        public string Stat { get; set; } = string.Empty;

        [JsonProperty("target")]
        public string Target { get; set; } = string.Empty;

        [JsonProperty("value")]
        public float Value { get; set; } = 1f;

        [JsonProperty("duration")]
        public string Duration { get; set; } = "instant";

        [JsonProperty("stackable")]
        public bool Stackable { get; set; } = false;

        [JsonProperty("maxTriggers")]
        public int MaxTriggers { get; set; } = -1;

        public ArtifactEffectType ParseType()
        {
            return Type?.ToLowerInvariant() switch
            {
                "stat_buff" => ArtifactEffectType.StatBuff,
                "all_stats_buff" => ArtifactEffectType.AllStatsBuff,
                "heal" => ArtifactEffectType.Heal,
                "shield" => ArtifactEffectType.Shield,
                "reflect_damage" => ArtifactEffectType.ReflectDamage,
                "revive" => ArtifactEffectType.Revive,
                "extra_choice" => ArtifactEffectType.ExtraChoice,
                _ => ArtifactEffectType.None
            };
        }

        public ArtifactBuffDuration ParseDuration()
        {
            return Duration?.ToLowerInvariant() switch
            {
                "battle" => ArtifactBuffDuration.Battle,
                "permanent" => ArtifactBuffDuration.Permanent,
                _ => ArtifactBuffDuration.Instant
            };
        }
    }

    /// <summary>
    /// Artifact configuration loaded from JSON.
    /// </summary>
    [Serializable]
    public sealed class ArtifactConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("rarity")]
        public string Rarity { get; set; } = "common";

        [JsonProperty("trigger")]
        public string Trigger { get; set; } = "passive";

        [JsonProperty("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonProperty("effect")]
        public ArtifactEffectConfig Effect { get; set; } = new();

        public ArtifactRarity ParseRarity()
        {
            return Rarity?.ToLowerInvariant() switch
            {
                "rare" => ArtifactRarity.Rare,
                "legendary" => ArtifactRarity.Legendary,
                _ => ArtifactRarity.Common
            };
        }

        public ArtifactTrigger ParseTrigger()
        {
            return Trigger?.ToLowerInvariant() switch
            {
                "on_battle_start" => ArtifactTrigger.OnBattleStart,
                "on_battle_end" => ArtifactTrigger.OnBattleEnd,
                "on_unit_death" => ArtifactTrigger.OnUnitDeath,
                "on_unit_kill" => ArtifactTrigger.OnUnitKill,
                "on_ally_death" => ArtifactTrigger.OnAllyDeath,
                "on_damage_received" => ArtifactTrigger.OnDamageReceived,
                "on_reward" => ArtifactTrigger.OnReward,
                _ => ArtifactTrigger.Passive
            };
        }
    }
}
