namespace Project.Core.Core.Combat.Effects
{
    /// <summary>
    /// Sink for combat effects. Allows effects to add follow-up effects.
    /// </summary>
    public interface ICombatEffectSink
    {
        /// <summary>
        /// Add a follow-up combat effect.
        /// </summary>
        void AddEffect(ICombatEffect effect);
    }

    /// <summary>
    /// Base interface for combat effects.
    /// Effects are created during combat resolution and applied sequentially.
    /// </summary>
    public interface ICombatEffect
    {
        /// <summary>
        /// Priority for execution order. Lower = first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Apply this effect synchronously.
        /// - Mutate domain state (HP, position, etc.)
        /// - Record visual events
        /// - Add follow-up effects via context.AddEffect()
        /// </summary>
        void Apply(ICombatEffectSink context);
    }
}
