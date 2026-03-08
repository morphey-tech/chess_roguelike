namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Result of trigger execution that controls the event flow.
    /// </summary>
    public enum TriggerResult
    {
        /// <summary>
        /// Continue processing other triggers and apply the event normally.
        /// </summary>
        Continue = 0,

        /// <summary>
        /// Stop processing other triggers but apply the event normally.
        /// </summary>
        Stop = 1,

        /// <summary>
        /// Cancel the event entirely. No further triggers are processed.
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// Replace the event with modified context. Requires context modification.
        /// </summary>
        Replace = 3
    }

    /// <summary>
    /// Base interface for all triggerable effects.
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Priority for execution order. Lower = first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Group within a phase. Controls execution order within the same phase.
        /// Default is TriggerGroup.Default which executes with other default triggers.
        /// </summary>
        TriggerGroup Group { get; }

        /// <summary>
        /// Phase for this trigger. Default is TriggerPhase.Default.
        /// </summary>
        TriggerPhase Phase { get; }

        /// <summary>
        /// Check if this trigger matches the current context.
        /// Fast filter before Execute is called.
        /// </summary>
        bool Matches(TriggerContext context);

        /// <summary>
        /// Execute the trigger with context.
        /// Called only if Matches() returns true.
        /// </summary>
        /// <returns>Result controlling the event flow.</returns>
        TriggerResult Execute(TriggerContext context);
    }
}