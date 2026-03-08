using System;
using System.Collections.Generic;
using Project.Core.Core.Logging;

namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Executes triggers with recursion protection, caching, and deterministic ordering.
    /// </summary>
    public sealed class TriggerExecutor<T>
    {
        private readonly Dictionary<(TriggerType, TriggerPhase), List<TriggerEntry>> _triggerMap = new();
        private readonly Func<IReadOnlyList<T>> _sourceProvider;
        private readonly ILogger _logger;

        private bool _isDirty = true;

        /// <summary>
        /// Global registration counter for deterministic ordering.
        /// </summary>
        private static int _globalRegistrationCounter;

        /// <summary>
        /// Maximum trigger chain depth to prevent infinite loops.
        /// Default: 10 (enough for most scenarios).
        /// </summary>
        public int MaxTriggerDepth { get; set; } = 10;

        /// <summary>
        /// Current execution depth (thread-local).
        /// </summary>
        [ThreadStatic]
        private static int _currentDepth;

        /// <summary>
        /// Track which triggers are currently executing (thread-local).
        /// </summary>
        [ThreadStatic]
        private static HashSet<string>? _executingTriggers;

        public TriggerExecutor(Func<IReadOnlyList<T>> sourceProvider, ILogService logService)
        {
            _sourceProvider = sourceProvider;
            _logger = logService.CreateLogger<TriggerExecutor<T>>();
        }

        public TriggerResult Execute(TriggerType type, TriggerContext context)
        {
            return Execute(type, context.Phase, context);
        }

        public TriggerResult Execute(TriggerType type, TriggerPhase phase, TriggerContext context)
        {
            // Check recursion depth
            if (_currentDepth >= MaxTriggerDepth)
            {
                _logger.Error($"Trigger execution depth exceeded {MaxTriggerDepth}. Possible infinite loop. Aborting.");
                return TriggerResult.Cancel;
            }

            if (_isDirty)
            {
                RebuildCache();
            }

            // Get triggers for this type and phase
            (TriggerType, TriggerPhase) key = (type, phase);
            if (!_triggerMap.TryGetValue(key, out List<TriggerEntry>? list))
            {
                return TriggerResult.Continue;
            }

            _currentDepth++;
            TriggerResult finalResult = TriggerResult.Continue;

            try
            {
                foreach (TriggerEntry? entry in list)
                {
                    try
                    {
                        // Check if this trigger is already executing (direct recursion)
                        string triggerId = entry.Source?.GetType().FullName ?? "Unknown";
                        if (_executingTriggers != null && _executingTriggers.Contains(triggerId))
                        {
                            _logger.Warning($"Recursive trigger detected: {triggerId}. Skipping.");
                            continue;
                        }

                        if (entry.Trigger.Matches(context))
                        {
                            // Mark as executing
                            _executingTriggers ??= new HashSet<string>();
                            _executingTriggers.Add(triggerId);

                            TriggerResult result = entry.Trigger.Execute(context);

                            // Remove from executing
                            _executingTriggers.Remove(triggerId);

                            // Handle flow control
                            switch (result)
                            {
                                case TriggerResult.Cancel:
                                    _logger.Debug($"Trigger {entry.Source} cancelled the event");
                                    finalResult = TriggerResult.Cancel;
                                    goto end;

                                case TriggerResult.Stop:
                                    _logger.Debug($"Trigger {entry.Source} stopped further triggers");
                                    finalResult = TriggerResult.Stop;
                                    goto end;

                                case TriggerResult.Replace:
                                    _logger.Debug($"Trigger {entry.Source} replaced the event");
                                    // Continue processing with modified context
                                    break;

                                case TriggerResult.Continue:
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in trigger {entry.Source}: {ex.Message}");
                    }
                }
            }
            finally
            {
                _currentDepth--;
            }

            end:
            return finalResult;
        }

        private void RebuildCache()
        {
            _triggerMap.Clear();
            _isDirty = false;

            IReadOnlyList<T>? sources = _sourceProvider();
            foreach (T source in sources)
            {
                if (source is ITrigger trigger)
                {
                    CacheTrigger(source, trigger);
                }
            }

            // Sort each group by Priority, then by Group, then by RegistrationOrder (deterministic)
            foreach (KeyValuePair<(TriggerType, TriggerPhase), List<TriggerEntry>> kvp in _triggerMap)
            {
                kvp.Value.Sort((a, b) =>
                {
                    // First: Priority (lower = first)
                    int priorityCompare = a.Trigger.Priority.CompareTo(b.Trigger.Priority);
                    if (priorityCompare != 0)
                        return priorityCompare;

                    // Second: Group (Additive → Multiplicative → Reduction → Final)
                    int groupCompare = a.Trigger.Group.CompareTo(b.Trigger.Group);
                    if (groupCompare != 0)
                        return groupCompare;

                    // Third: RegistrationOrder (earlier = first)
                    return a.RegistrationOrder.CompareTo(b.RegistrationOrder);
                });
            }

            _logger.Debug($"Trigger cache rebuilt: {_triggerMap.Count} trigger type/phase combinations");
        }

        private void CacheTrigger(T source, ITrigger trigger)
        {
            TriggerType triggerType = GetTriggerType(trigger);
            if (triggerType == TriggerType.None)
            {
                return;
            }

            // Use trigger's specific phase or Default
            TriggerPhase triggerPhase = trigger.Phase;

            (TriggerType, TriggerPhase) key = (triggerType, triggerPhase);
            if (!_triggerMap.ContainsKey(key))
            {
                _triggerMap[key] = new List<TriggerEntry>();
            }

            // Assign registration order at cache time
            int registrationOrder = System.Threading.Interlocked.Increment(ref _globalRegistrationCounter);
            _triggerMap[key].Add(new TriggerEntry(source, trigger, registrationOrder));
        }

        private static TriggerType GetTriggerType(ITrigger trigger)
        {
            return trigger switch
            {
                IOnBeforeHit => TriggerType.OnBeforeHit,
                IOnAfterHit => TriggerType.OnAfterHit,
                IOnBattleStart => TriggerType.OnBattleStart,
                IOnBattleEnd => TriggerType.OnBattleEnd,
                IOnUnitKill => TriggerType.OnUnitKill,
                IOnUnitDeath => TriggerType.OnUnitDeath,
                IOnAllyDeath => TriggerType.OnAllyDeath,
                IOnDamageReceived => TriggerType.OnDamageReceived,
                IOnDamageDealt => TriggerType.OnDamageDealt,
                IOnAttack => TriggerType.OnAttack,
                IOnTurnStart => TriggerType.OnTurnStart,
                IOnTurnEnd => TriggerType.OnTurnEnd,
                IOnMove => TriggerType.OnMove,
                IOnReward => TriggerType.OnReward,
                IOnRunStart => TriggerType.OnRunStart,
                IOnStageEnter => TriggerType.OnStageEnter,
                IOnStageLeave => TriggerType.OnStageLeave,
                _ => TriggerType.None
            };
        }

        public void InvalidateCache()
        {
            _isDirty = true;
        }

        private sealed class TriggerEntry
        {
            public T Source { get; }
            public ITrigger Trigger { get; }
            public int RegistrationOrder { get; }

            public TriggerEntry(T source, ITrigger trigger, int registrationOrder)
            {
                Source = source;
                Trigger = trigger;
                RegistrationOrder = registrationOrder;
            }
        }
    }
}
