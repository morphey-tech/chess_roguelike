using Project.Core.Core.Triggers;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Base interface for all passive abilities.
    /// Extends ITrigger for unified execution.
    /// </summary>
    public interface IPassive : ITrigger
    {
        /// <summary>
        /// Unique identifier for this passive.
        /// </summary>
        string Id { get; }
    }
}
