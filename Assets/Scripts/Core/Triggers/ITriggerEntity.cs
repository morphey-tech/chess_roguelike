namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Base interface for entities that can participate in trigger events.
    /// Provides type-safe access to Actor and Target in TriggerContext.
    /// </summary>
    public interface ITriggerEntity
    {
        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        string Id { get; }
    }
}
