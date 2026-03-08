namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Phase of a trigger event. Allows multiple execution points within a single event type.
    /// Example: OnBeforeHit has phases: Prepare → Modify → Finalize
    /// </summary>
    public enum TriggerPhase
    {
        /// <summary>
        /// Default phase. Used when no specific phase is required.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Before damage calculation. Used for buffs, debuffs, conditions.
        /// Example: Critical hit check, damage multipliers.
        /// </summary>
        BeforeCalculation = 1,

        /// <summary>
        /// During damage calculation. Used for modifications.
        /// Example: Armor penetration, resistance, vulnerability.
        /// </summary>
        ModifyCalculation = 2,

        /// <summary>
        /// After damage calculation but before application.
        /// Example: Shield absorption, damage redirection.
        /// </summary>
        BeforeApplication = 3,

        /// <summary>
        /// After damage is applied.
        /// Example: Lifesteal, thorns, on-damage effects.
        /// </summary>
        AfterApplication = 4,

        /// <summary>
        /// Before attack is declared. Used for cancel conditions.
        /// Example: Sleep, stun, freeze checks.
        /// </summary>
        BeforeDeclare = 10,

        /// <summary>
        /// When attack is declared but not yet resolved.
        /// Example: Target selection, range validation.
        /// </summary>
        OnDeclare = 11,

        /// <summary>
        /// After attack is declared, before hit resolution.
        /// Example: Dodge check, block check, parry.
        /// </summary>
        AfterDeclare = 12,

        /// <summary>
        /// Before hit is resolved.
        /// Example: Damage calculation, critical check.
        /// </summary>
        BeforeHit = 13,

        /// <summary>
        /// After hit is resolved.
        /// Example: On-hit effects, poison application.
        /// </summary>
        AfterHit = 14,

        /// <summary>
        /// Before death is processed. Can prevent death.
        /// Example: Revive, death prevention, last stand.
        /// </summary>
        BeforeDeath = 20,

        /// <summary>
        /// When death is being processed.
        /// Example: Death effects, soul collection.
        /// </summary>
        OnDeath = 21,

        /// <summary>
        /// After death is processed.
        /// Example: Corpse effects, resurrection cooldowns.
        /// </summary>
        AfterDeath = 22,

        /// <summary>
        /// Before turn starts. Used for preparation.
        /// Example: Draw cards, gain mana, clear debuffs.
        /// </summary>
        BeforeTurn = 30,

        /// <summary>
        /// When turn starts.
        /// Example: Start of turn effects, buffs tick.
        /// </summary>
        OnTurnStart = 31,

        /// <summary>
        /// During turn, before action.
        /// Example: Action point calculation, movement buffs.
        /// </summary>
        DuringTurn = 32,

        /// <summary>
        /// When turn ends.
        /// Example: End of turn effects, damage over time.
        /// </summary>
        OnTurnEnd = 33,

        /// <summary>
        /// After turn ends. Cleanup phase.
        /// Example: Expire buffs, cleanup temporary effects.
        /// </summary>
        AfterTurn = 34,

        /// <summary>
        /// Before movement starts.
        /// Example: Movement cost calculation, path validation.
        /// </summary>
        BeforeMove = 40,

        /// <summary>
        /// During movement.
        /// Example: Opportunity attacks, terrain effects.
        /// </summary>
        DuringMove = 41,

        /// <summary>
        /// After movement completes.
        /// Example: On-move effects, position-based buffs.
        /// </summary>
        AfterMove = 42,

        /// <summary>
        /// First phase. Earliest execution.
        /// </summary>
        First = 100,

        /// <summary>
        /// Second phase. Early execution.
        /// </summary>
        Second = 200,

        /// <summary>
        /// Third phase. Middle execution.
        /// </summary>
        Third = 300,

        /// <summary>
        /// Last phase. Latest execution.
        /// </summary>
        Last = 1000,
    }
}
