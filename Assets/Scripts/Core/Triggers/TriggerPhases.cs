namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Constants for common phase combinations.
    /// </summary>
    public static class TriggerPhases
    {
        /// <summary>
        /// All damage-related phases in order.
        /// </summary>
        public static readonly TriggerPhase[] DamagePipeline =
        {
            TriggerPhase.BeforeCalculation,
            TriggerPhase.ModifyCalculation,
            TriggerPhase.BeforeApplication,
            TriggerPhase.AfterApplication
        };

        /// <summary>
        /// All attack-related phases in order.
        /// </summary>
        public static readonly TriggerPhase[] AttackPipeline =
        {
            TriggerPhase.BeforeDeclare,
            TriggerPhase.OnDeclare,
            TriggerPhase.AfterDeclare,
            TriggerPhase.BeforeHit,
            TriggerPhase.AfterHit
        };

        /// <summary>
        /// All death-related phases in order.
        /// </summary>
        public static readonly TriggerPhase[] DeathPipeline =
        {
            TriggerPhase.BeforeDeath,
            TriggerPhase.OnDeath,
            TriggerPhase.AfterDeath
        };

        /// <summary>
        /// All turn-related phases in order.
        /// </summary>
        public static readonly TriggerPhase[] TurnPipeline =
        {
            TriggerPhase.BeforeTurn,
            TriggerPhase.OnTurnStart,
            TriggerPhase.DuringTurn,
            TriggerPhase.OnTurnEnd,
            TriggerPhase.AfterTurn
        };

        /// <summary>
        /// All movement-related phases in order.
        /// </summary>
        public static readonly TriggerPhase[] MovementPipeline =
        {
            TriggerPhase.BeforeMove,
            TriggerPhase.DuringMove,
            TriggerPhase.AfterMove
        };
    }
}