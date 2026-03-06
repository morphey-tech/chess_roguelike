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

        [JsonProperty("radius")]
        public float Radius { get; set; } = 1f;

        [JsonProperty("params")]
        public System.Collections.Generic.Dictionary<string, float> Params { get; set; } = new();
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

        /// <summary>
        /// Maximum stack count for this artifact.
        /// -1 = unlimited, 1 = not stackable, N = max N stacks
        /// Default: 1 (not stackable)
        /// </summary>
        [JsonProperty("max_stack")]
        public int MaxStack { get; set; } = 1;

        /// <summary>
        /// Comma-separated tags (e.g. "attack,utility").
        /// </summary>
        [JsonProperty("tags")]
        public string Tags { get; set; } = string.Empty;

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

        /// <summary>
        /// Parse tags from comma-separated string.
        /// </summary>
        public ArtifactTag ParseTags()
        {
            if (string.IsNullOrEmpty(Tags))
                return ArtifactTag.None;

            ArtifactTag result = ArtifactTag.None;
            var tagList = Tags.Split(',');

            foreach (var tagStr in tagList)
            {
                var trimmed = tagStr.Trim().ToLowerInvariant();
                if (Enum.TryParse<ArtifactTag>(trimmed, true, out var tag))
                {
                    result |= tag;
                }
            }

            return result;
        }

        /// <summary>
        /// Check if artifact can be stacked.
        /// </summary>
        public bool IsStackable() => MaxStack > 1 || MaxStack == -1;

        /// <summary>
        /// Get maximum stack count (-1 = unlimited).
        /// </summary>
        public int GetMaxStack() => MaxStack;
    }
}
