using System;

namespace Project.Core.Core.Configs.Artifacts
{
    /// <summary>
    /// Rarity tiers for artifacts. Affects drop rates and power.
    /// </summary>
    public enum ArtifactRarity
    {
        Common = 0,
        Rare = 1,
        Legendary = 2
    }

    /// <summary>
    /// Trigger types for artifact effects.
    /// </summary>
    public enum ArtifactTrigger
    {
        /// <summary>No trigger, always active (e.g. +stats).</summary>
        Passive = 0,

        /// <summary>At battle/combat start.</summary>
        OnBattleStart = 1,

        /// <summary>At battle/combat end (victory).</summary>
        OnBattleEnd = 2,

        /// <summary>When any unit dies.</summary>
        OnUnitDeath = 3,

        /// <summary>When this figure kills an enemy.</summary>
        OnUnitKill = 4,

        /// <summary>When an ally dies.</summary>
        OnAllyDeath = 5,

        /// <summary>When receiving damage.</summary>
        OnDamageReceived = 6,

        /// <summary>At reward selection (loot choice).</summary>
        OnReward = 7
    }

    /// <summary>
    /// Effect types for artifacts.
    /// </summary>
    public enum ArtifactEffectType
    {
        None = 0,

        /// <summary>Buff a stat (attack, hp, movement, etc.).</summary>
        StatBuff = 1,

        /// <summary>Buff all stats.</summary>
        AllStatsBuff = 2,

        /// <summary>Heal target.</summary>
        Heal = 3,

        /// <summary>Grant shield.</summary>
        Shield = 4,

        /// <summary>Reflect damage back to attacker.</summary>
        ReflectDamage = 5,

        /// <summary>Revive a fallen ally.</summary>
        Revive = 6,

        /// <summary>Extra choice at reward selection.</summary>
        ExtraChoice = 7
    }

    /// <summary>
    /// Duration for stat buffs.
    /// </summary>
    public enum ArtifactBuffDuration
    {
        Instant = 0,
        Battle = 1,
        Permanent = 2
    }
}
