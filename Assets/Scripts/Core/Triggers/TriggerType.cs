namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Types of triggers supported by the system.
    /// </summary>
    public enum TriggerType
    {
        None = 0,

        // Combat - Hit/Death
        OnBattleStart = 1,
        OnBattleEnd = 2,
        OnUnitKill = 3,
        OnUnitDeath = 4,
        OnAllyDeath = 5,
        OnDamageReceived = 6,
        OnDamageDealt = 7,
        OnAttack = 8,
        OnBeforeHit = 16,
        OnAfterHit = 17,

        // Turn
        OnTurnStart = 9,
        OnTurnEnd = 10,
        OnMove = 11,

        // Run-level
        OnReward = 12,
        OnRunStart = 13,
        OnStageEnter = 14,
        OnStageLeave = 15
    }
}
