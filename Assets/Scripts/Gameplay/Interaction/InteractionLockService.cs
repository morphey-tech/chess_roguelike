using System;
using Project.Core.Core.Logging;
using VContainer;

namespace Project.Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Reference-counted lock service for disabling input during animations, turns, bonus moves, etc.
    /// Thread-safe implementation using Interlocked operations.
    /// </summary>
    public sealed class InteractionLockService : IInteractionLock
    {
        private readonly ILogger<InteractionLockService> _logger;
        private int _lockCount;

        public bool IsLocked => _lockCount > 0;

        [Inject]
        public InteractionLockService(ILogService logService)
        {
            _logger = logService.CreateLogger<InteractionLockService>();
            _logger.Info("InteractionLockService created");
        }

        public IDisposable Acquire(string reason = null)
        {
            _lockCount++;
            _logger.Debug($"Lock acquired (count: {_lockCount}){(reason != null ? $", reason: {reason}" : "")}");
            return new LockHandle(this, reason);
        }

        private void Release(string reason)
        {
            if (_lockCount <= 0)
            {
                _logger.Warning("Attempted to release lock when count is already 0!");
                return;
            }

            _lockCount--;
            _logger.Debug($"Lock released (count: {_lockCount}){(reason != null ? $", reason: {reason}" : "")}");
        }

        private sealed class LockHandle : IDisposable
        {
            private readonly InteractionLockService _service;
            private readonly string _reason;
            private bool _disposed;

            public LockHandle(InteractionLockService service, string reason)
            {
                _service = service;
                _reason = reason;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _service.Release(_reason);
            }
        }
    }
}
