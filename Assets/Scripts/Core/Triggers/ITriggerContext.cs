using Project.Core.Core.Combat;
using Project.Core.Core.Grid;

namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Base interface for all trigger contexts.
    /// </summary>
    public interface ITriggerContext
    {
        /// <summary>
        /// Type of trigger event.
        /// </summary>
        TriggerType Type { get; }

        /// <summary>
        /// Phase within the event type.
        /// </summary>
        TriggerPhase Phase { get; }

        /// <summary>
        /// Main actor (killer, attacker, moving figure, etc.).
        /// </summary>
        ITriggerEntity? Actor { get; }

        /// <summary>
        /// Target of the action (victim, defender, etc.).
        /// </summary>
        ITriggerEntity? Target { get; }

        /// <summary>
        /// Optional additional data.
        /// </summary>
        object? Data { get; }
    }

    /// <summary>
    /// Context for damage-related triggers (OnBeforeHit, OnAfterHit, OnDamageReceived, OnDamageDealt).
    /// </summary>
    public interface IDamageContext : ITriggerContext
    {
        /// <summary>
        /// Base damage before modifiers.
        /// </summary>
        float BaseValue { get; }

        /// <summary>
        /// Current damage after modifiers.
        /// </summary>
        float CurrentValue { get; set; }

        /// <summary>
        /// Damage multiplier (crit, buffs, etc.).
        /// </summary>
        float DamageMultiplier { get; set; }

        /// <summary>
        /// Flat bonus damage added to base.
        /// </summary>
        float BonusDamage { get; set; }

        /// <summary>
        /// Is this hit a critical?
        /// </summary>
        bool IsCritical { get; set; }

        /// <summary>
        /// Was this hit dodged?
        /// </summary>
        bool IsDodged { get; set; }

        /// <summary>
        /// Should this hit be cancelled?
        /// </summary>
        bool IsCancelled { get; set; }
    }

    /// <summary>
    /// Context for movement triggers (OnMove).
    /// </summary>
    public interface IMoveContext : ITriggerContext
    {
        /// <summary>
        /// Starting position.
        /// </summary>
        GridPosition From { get; }

        /// <summary>
        /// Destination position.
        /// </summary>
        GridPosition To { get; }

        /// <summary>
        /// Did the figure actually move?
        /// </summary>
        bool DidMove { get; }
    }

    /// <summary>
    /// Context for turn triggers (OnTurnStart, OnTurnEnd).
    /// </summary>
    public interface ITurnContext : ITriggerContext
    {
        /// <summary>
        /// Current turn number.
        /// </summary>
        int TurnNumber { get; }

        /// <summary>
        /// Team whose turn it is.
        /// </summary>
        Team Team { get; }
    }

    /// <summary>
    /// Context for battle triggers (OnBattleStart, OnBattleEnd).
    /// </summary>
    public interface IBattleContext : ITriggerContext
    {
    }

    /// <summary>
    /// Context for kill/death triggers (OnUnitKill, OnUnitDeath, OnAllyDeath).
    /// </summary>
    public interface IKillContext : ITriggerContext
    {
        /// <summary>
        /// The figure that died.
        /// </summary>
        ITriggerEntity? Victim { get; }

        /// <summary>
        /// The figure that caused the death (if any).
        /// </summary>
        ITriggerEntity? Killer { get; }
    }

    /// <summary>
    /// Context for reward triggers (OnReward).
    /// </summary>
    public interface IRewardContext : ITriggerContext
    {
        /// <summary>
        /// Reward type or ID.
        /// </summary>
        string? RewardId { get; }
    }

    /// <summary>
    /// Context for run-level triggers (OnRunStart, OnStageEnter, OnStageLeave).
    /// </summary>
    public interface IRunContext : ITriggerContext
    {
        /// <summary>
        /// Current stage or run ID.
        /// </summary>
        string? StageId { get; }
    }

    /// <summary>
    /// Extension methods for trigger contexts.
    /// </summary>
    public static class TriggerContextExtensions
    {
        /// <summary>
        /// Try to get typed data from context.
        /// </summary>
        public static bool TryGetData<T>(this ITriggerContext context, out T? data) where T : class
        {
            data = context.Data as T;
            return data != null;
        }
    }
}
