namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Group within a phase. Controls execution order within the same phase.
    /// Useful for ordering modifiers: Additive → Multiplicative → Reduction.
    /// </summary>
    public enum TriggerGroup
    {
        /// <summary>
        /// Default group. Executes with other default triggers.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Additive modifiers (flat bonuses).
        /// Example: +5 damage, +10 armor.
        /// Executed FIRST in Modify phase.
        /// </summary>
        Additive = 1,

        /// <summary>
        /// Multiplicative modifiers (percent bonuses).
        /// Example: x2 damage, x1.5 armor.
        /// Executed SECOND in Modify phase.
        /// </summary>
        Multiplicative = 2,

        /// <summary>
        /// Reduction modifiers (flat/percent reduction).
        /// Example: -3 damage, -50% damage taken.
        /// Executed THIRD in Modify phase.
        /// </summary>
        Reduction = 3,

        /// <summary>
        /// Final modifiers (cap, clamp, etc.).
        /// Example: min 1 damage, max 999 damage.
        /// Executed LAST in Modify phase.
        /// </summary>
        Final = 4,

        /// <summary>
        /// First group in a phase.
        /// </summary>
        First = 10,

        /// <summary>
        /// Early group in a phase.
        /// </summary>
        Early = 20,

        /// <summary>
        /// Normal group in a phase.
        /// </summary>
        Normal = 30,

        /// <summary>
        /// Late group in a phase.
        /// </summary>
        Late = 40,

        /// <summary>
        /// Last group in a phase.
        /// </summary>
        Last = 50,
    }
}
