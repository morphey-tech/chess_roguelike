using System;
using Project.Core.Core.Triggers;

namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Extension methods for trigger registration.
    /// </summary>
    public static class TriggerRegistrationExtensions
    {
        private static readonly object _registrationLock = new();

        /// <summary>
        /// Register a trigger with automatic unregistration on dispose.
        /// Returns a disposable that unregisters the trigger when disposed.
        /// </summary>
        public static IDisposable RegisterDisposable(this TriggerService service, ITrigger trigger)
        {
            service.Register(trigger);
            return new TriggerUnregisterAction(service, trigger);
        }

        /// <summary>
        /// Execute a trigger directly without registering it.
        /// Useful for one-time triggers.
        /// </summary>
        public static TriggerResult ExecuteOnce(this TriggerService service, TriggerType type, TriggerContext context, ITrigger trigger)
        {
            if (!trigger.Matches(context))
            {
                return TriggerResult.Continue;
            }
            return trigger.Execute(context);
        }

        private sealed class TriggerUnregisterAction : IDisposable
        {
            private readonly TriggerService _service;
            private readonly ITrigger _trigger;
            private bool _disposed;

            public TriggerUnregisterAction(TriggerService service, ITrigger trigger)
            {
                _service = service;
                _trigger = trigger;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _service.Unregister(_trigger);
                    _disposed = true;
                }
            }
        }
    }

    /// <summary>
    /// Base class for objects that own triggers and manage their registration.
    /// Automatically registers triggers on construction and unregisters on dispose.
    /// </summary>
    public abstract class TriggerOwner : IDisposable
    {
        private readonly TriggerService _triggerService;
        private readonly ITrigger[] _triggers;
        private bool _disposed;

        protected TriggerOwner(TriggerService triggerService, params ITrigger[] triggers)
        {
            _triggerService = triggerService;
            _triggers = triggers;

            foreach (var trigger in _triggers)
            {
                _triggerService.Register(trigger);
            }
        }

        /// <summary>
        /// Called when a trigger is executed.
        /// Override to add custom logic before/after execution.
        /// </summary>
        protected virtual void OnTriggerExecuted(ITrigger trigger, TriggerResult result)
        {
        }

        /// <summary>
        /// Execute a single trigger directly (bypasses the service).
        /// Useful for owner-specific triggers.
        /// </summary>
        protected TriggerResult ExecuteLocal(TriggerContext context, ITrigger trigger)
        {
            if (_disposed)
            {
                return TriggerResult.Continue;
            }

            if (!trigger.Matches(context))
            {
                return TriggerResult.Continue;
            }

            TriggerResult result = trigger.Execute(context);
            OnTriggerExecuted(trigger, result);
            return result;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var trigger in _triggers)
                {
                    _triggerService.Unregister(trigger);
                }
                _disposed = true;
            }
        }
    }
}
