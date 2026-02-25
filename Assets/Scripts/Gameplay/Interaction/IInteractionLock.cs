using System;

namespace Project.Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Reference-counted lock for disabling input during animations, turns, bonus moves, etc.
    /// </summary>
    public interface IInteractionLock
    {
        /// <summary>
        /// Returns true if input should be blocked.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Acquires a lock. Dispose the returned handle to release.
        /// Multiple acquisitions are supported (reference-counted).
        /// </summary>
        /// <param name="reason">Optional reason for debugging/logging.</param>
        IDisposable Acquire(string? reason = null);
    }
}
