namespace Project.Core.Core.Configs.Artifacts
{
    /// <summary>
    /// Rarity tiers for artifacts. Affects drop rates.
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
        Passive = 0,
        OnBattleStart = 1,
        OnBattleEnd = 2,
        OnUnitDeath = 3,
        OnUnitKill = 4,
        OnAllyDeath = 5,
        OnDamageReceived = 6,
        OnReward = 7
    }
}
