using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Combat.Effects
{
    /// <summary>
    /// Represents a single effect produced by combat (damage, heal, push, etc.)
    /// Effects are created during combat resolution and applied sequentially by AttackStep.
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
        /// Apply this effect: update visuals, publish events, modify context.
        /// </summary>
        UniTask ApplyAsync(CombatEffectContext context);
    }
}
