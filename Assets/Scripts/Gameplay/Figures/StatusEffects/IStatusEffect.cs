using Project.Core.Core.Triggers;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Base interface for all status effects.
    /// Extends ITrigger for unified execution.
    /// </summary>
    public interface IStatusEffect : ITrigger
    {
        /// <summary>
        /// Unique identifier for this effect.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Is this effect expired and should be removed?
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Called when effect is first applied.
        /// </summary>
        void OnApply(Figure owner);

        /// <summary>
        /// Called when effect is removed.
        /// </summary>
        void OnRemove(Figure owner);
    }
}