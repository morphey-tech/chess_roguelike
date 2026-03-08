using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Core.Core.Triggers;
using VContainer;

namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Central trigger registry and executor.
    /// Does NOT know about Figures, Artifacts, or StatusEffects.
    /// </summary>
    public sealed class TriggerService : IDisposable
    {
        private readonly ILogService _logService;
        private readonly List<ITrigger> _triggers = new();
        private readonly object _lock = new();
        private TriggerExecutor<ITrigger>? _executor;
        private bool _isDirty = true;

        [Inject]
        public TriggerService(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Register a trigger for execution.
        /// </summary>
        public void Register(ITrigger trigger)
        {
            lock (_lock)
            {
                if (!_triggers.Contains(trigger))
                {
                    _triggers.Add(trigger);
                    _isDirty = true;
                }
            }
        }

        /// <summary>
        /// Unregister a trigger from execution.
        /// </summary>
        public void Unregister(ITrigger trigger)
        {
            lock (_lock)
            {
                if (_triggers.Remove(trigger))
                {
                    _isDirty = true;
                }
            }
        }

        /// <summary>
        /// Execute all triggers matching the context.
        /// </summary>
        public TriggerResult Execute(TriggerType type, TriggerContext context)
        {
            return Execute(type, context.Phase, context);
        }

        /// <summary>
        /// Execute all triggers matching the type and phase.
        /// </summary>
        public TriggerResult Execute(TriggerType type, TriggerPhase phase, TriggerContext context)
        {
            List<ITrigger> triggers;
            lock (_lock)
            {
                triggers = _triggers.Where(t => t.Matches(context))
                                   .OrderBy(t => t.Priority)
                                   .ToList();
            }

            if (triggers.Count == 0)
            {
                return TriggerResult.Continue;
            }

            if (_executor == null || _isDirty)
            {
                _executor = new TriggerExecutor<ITrigger>(
                    () => triggers,
                    _logService);
                _isDirty = false;
            }
            else
            {
                _executor.InvalidateCache();
            }

            return _executor.Execute(type, phase, context);
        }

        public void Dispose()
        {
            _triggers.Clear();
            _isDirty = true;
        }
    }
}
