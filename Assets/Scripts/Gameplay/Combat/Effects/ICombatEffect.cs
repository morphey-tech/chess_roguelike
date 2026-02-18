namespace Project.Gameplay.Gameplay.Combat.Effects
{
    /// <summary>
    /// Represents a single effect produced by combat (damage, heal, push, etc.)
    /// Effects are created during combat resolution and applied sequentially by AttackAction.
    /// 
    /// DESIGN RULES:
    /// - Effects are SYNCHRONOUS (no async) - they only mutate domain state
    /// - Effects record visual events via context.AddVisualEvent()
    /// - Visuals are executed AFTER all effects complete
    /// - This ensures predictable ordering and no race conditions
    /// </summary>
    public interface ICombatEffect
    {
        /// <summary>
        /// The phase in which this effect executes.
        /// Effects are sorted by Phase first, then by OrderInPhase.
        /// </summary>
        CombatEffectPhase Phase { get; }
        
        /// <summary>
        /// Order within the phase. Lower values execute first.
        /// Use 0 for default, 10/20/30 for explicit ordering.
        /// </summary>
        int OrderInPhase { get; }
        
        /// <summary>
        /// Apply this effect synchronously.
        /// - Mutate domain state (HP, position, etc.)
        /// - Record visual events via context.AddVisualEvent()
        /// - Add follow-up effects via context.AddEffect()
        /// </summary>
        void Apply(CombatEffectContext context);
    }
}
