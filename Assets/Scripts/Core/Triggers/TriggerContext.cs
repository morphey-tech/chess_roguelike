using System;
using System.Collections.Generic;
using System.Text;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using UnityEngine;

namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Universal context for all triggers.
    /// Core fields are immutable. Mutable data is separated.
    /// Implements all context interfaces for flexibility.
    /// </summary>
    public sealed class TriggerContext : ITriggerContext, IDamageContext, IMoveContext, ITurnContext, IKillContext, IBattleContext, IRewardContext, IRunContext
    {
        /// <summary>
        /// Type of trigger event. Immutable after creation.
        /// </summary>
        public TriggerType Type { get; internal set; }

        /// <summary>
        /// Phase within the event type. Immutable after creation.
        /// </summary>
        public TriggerPhase Phase { get; internal set; }

        /// <summary>
        /// Main actor (killer, attacker, moving figure, etc.). Immutable after creation.
        /// Type-safe: implements ITriggerEntity.
        /// </summary>
        public ITriggerEntity? Actor { get; internal set; }

        /// <summary>
        /// Target of the action (victim, defender, etc.). Immutable after creation.
        /// Type-safe: implements ITriggerEntity.
        /// </summary>
        public ITriggerEntity? Target { get; internal set; }

        /// <summary>
        /// Source type of the trigger. Immutable after creation.
        /// Used to prevent infinite loops and filter triggers.
        /// </summary>
        public TriggerSource SourceType { get; internal set; }

        /// <summary>
        /// Source object (specific trigger source). Immutable after creation.
        /// Can be used to check if this specific source caused the trigger.
        /// </summary>
        public object? SourceObject { get; internal set; }

        /// <summary>
        /// Base value (original, unmodified). Immutable after creation.
        /// Use MutableData for modifications.
        /// </summary>
        public float BaseValue { get; internal set; }

        internal TriggerContext() { }

        /// <summary>
        /// Current value (can be modified by triggers).
        /// </summary>
        public float CurrentValue { get; set; }

        /// <summary>
        /// Optional additional data.
        /// Can be modified in appropriate phases.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Stack count for stacking triggers.
        /// </summary>
        public int StackCount { get; set; }

        // IDamageContext properties
        public float DamageMultiplier { get; set; } = 1f;
        public float BonusDamage { get; set; }
        public bool IsCritical { get; set; }
        public bool IsDodged { get; set; }
        public bool IsCancelled { get; set; }

        // IMoveContext properties
        public GridPosition From { get; internal set; }
        public GridPosition To { get; internal set; }
        public bool DidMove { get; internal set; }

        // ITurnContext properties
        public int TurnNumber { get; internal set; }
        public Team Team { get; internal set; }

        // IKillContext properties
        ITriggerEntity? IKillContext.Victim => Target;
        ITriggerEntity? IKillContext.Killer => Actor;

        // IRewardContext properties
        public string? RewardId { get; internal set; }

        // IRunContext properties
        public string? StageId { get; internal set; }

        /// <summary>
        /// Type-safe custom data storage.
        /// </summary>
        private Dictionary<Type, object>? _typedData;

        /// <summary>
        /// Mutation log for debugging.
        /// </summary>
        private List<MutationRecord>? _mutationLog;

        /// <summary>
        /// Trace log for debugging value changes.
        /// Shows full pipeline of modifications.
        /// </summary>
        private List<TraceRecord>? _traceLog;

        /// <summary>
        /// Check if context has been modified.
        /// </summary>
        public bool IsModified => _mutationLog is { Count: > 0 };

        /// <summary>
        /// Total delta from all modifications.
        /// </summary>
        public float TotalDelta => CurrentValue - BaseValue;

        public static TriggerContext Create(TriggerType type)
        {
            return new TriggerContext { Type = type };
        }

        public static TriggerContext Create(TriggerType type, TriggerPhase phase)
        {
            return new TriggerContext { Type = type, Phase = phase };
        }

        public static TriggerContext Create(TriggerType type, TriggerSource sourceType)
        {
            return new TriggerContext { Type = type, SourceType = sourceType };
        }

        public static TriggerContext Create(TriggerType type, TriggerSource sourceType, object? sourceObject)
        {
            return new TriggerContext { Type = type, SourceType = sourceType, SourceObject = sourceObject };
        }

        public static TriggerContext Create(TriggerType type, TriggerPhase phase, TriggerSource sourceType)
        {
            return new TriggerContext { Type = type, Phase = phase, SourceType = sourceType };
        }

        public static TriggerContext Create(TriggerType type, TriggerPhase phase, TriggerSource sourceType, object? sourceObject)
        {
            return new TriggerContext { Type = type, Phase = phase, SourceType = sourceType, SourceObject = sourceObject };
        }

        public static TriggerContext Create(TriggerType type, ITriggerEntity actor)
        {
            return new TriggerContext { Type = type, Actor = actor };
        }

        public static TriggerContext Create(TriggerType type, TriggerPhase phase, ITriggerEntity actor)
        {
            return new TriggerContext { Type = type, Phase = phase, Actor = actor };
        }

        public static TriggerContext Create(TriggerType type, ITriggerEntity actor, ITriggerEntity target)
        {
            return new TriggerContext { Type = type, Actor = actor, Target = target };
        }

        public static TriggerContext Create(TriggerType type, TriggerPhase phase, ITriggerEntity actor, ITriggerEntity target)
        {
            return new TriggerContext { Type = type, Phase = phase, Actor = actor, Target = target };
        }

        public static TriggerContext Create(TriggerType type, ITriggerEntity actor, ITriggerEntity target, int baseValue)
        {
            return new TriggerContext 
            { 
                Type = type, 
                Actor = actor, 
                Target = target, 
                BaseValue = baseValue,
                CurrentValue = baseValue
            };
        }

        public static TriggerContext Create(TriggerType type, TriggerPhase phase, TriggerSource sourceType, ITriggerEntity actor, ITriggerEntity target, int baseValue)
        {
            return new TriggerContext 
            { 
                Type = type,
                Phase = phase,
                SourceType = sourceType,
                Actor = actor, 
                Target = target, 
                BaseValue = baseValue,
                CurrentValue = baseValue
            };
        }

        /// <summary>
        /// Get typed data from context.
        /// </summary>
        public T? GetData<T>() where T : class
        {
            return Data as T;
        }

        /// <summary>
        /// Try to get typed data from context.
        /// </summary>
        public bool TryGetData<T>(out T? data) where T : class
        {
            data = Data as T;
            return data != null;
        }

        /// <summary>
        /// Get typed custom data by type.
        /// </summary>
        public T? GetCustomData<T>() where T : class
        {
            if (_typedData != null && _typedData.TryGetValue(typeof(T), out var value))
            {
                return value as T;
            }
            return null;
        }

        /// <summary>
        /// Try to get typed custom data by type.
        /// </summary>
        public bool TryGetCustomData<T>(out T? data) where T : class
        {
            if (_typedData != null && _typedData.TryGetValue(typeof(T), out var value))
            {
                data = value as T;
                return data != null;
            }
            data = null;
            return false;
        }

        /// <summary>
        /// Set typed custom data.
        /// </summary>
        public void SetCustomData<T>(T value) where T : class
        {
            _typedData ??= new Dictionary<Type, object>();
            _typedData[typeof(T)] = value;
        }

        /// <summary>
        /// Check if custom data of type T exists.
        /// </summary>
        public bool HasCustomData<T>() where T : class
        {
            return _typedData != null && _typedData.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Modify CurrentValue with trace logging.
        /// </summary>
        public void ModifyValue(float delta, string source)
        {
            float oldValue = CurrentValue;
            CurrentValue += delta;
            LogMutation(source, delta, CurrentValue);
            AddTrace(source, oldValue, delta, CurrentValue);
        }

        /// <summary>
        /// Set CurrentValue with trace logging.
        /// </summary>
        public void SetValue(float newValue, string source)
        {
            float oldValue = CurrentValue;
            float delta = newValue - CurrentValue;
            CurrentValue = newValue;
            LogMutation(source, delta, CurrentValue);
            AddTrace(source, oldValue, delta, CurrentValue, isSet: true);
        }

        /// <summary>
        /// Multiply CurrentValue with trace logging.
        /// </summary>
        public void MultiplyValue(float multiplier, string source)
        {
            float oldValue = CurrentValue;
            CurrentValue = CurrentValue * multiplier;
            float delta = CurrentValue - oldValue;
            LogMutation(source, delta, CurrentValue);
            AddTrace(source, oldValue, delta, CurrentValue, multiplier: multiplier);
        }

        /// <summary>
        /// Reset CurrentValue to BaseValue.
        /// </summary>
        public void ResetValue()
        {
            CurrentValue = BaseValue;
        }

        private void LogMutation(string source, float delta, float newValue)
        {
            _mutationLog ??= new List<MutationRecord>();
            _mutationLog.Add(new MutationRecord
            {
                Source = source,
                Delta = delta,
                NewValue = newValue,
                Timestamp = DateTime.Now
            });
        }

        private void AddTrace(string source, float oldValue, float delta, float newValue, bool isSet = false, float multiplier = 1f)
        {
            _traceLog ??= new List<TraceRecord>();
            _traceLog.Add(new TraceRecord
            {
                Source = source,
                OldValue = oldValue,
                Delta = delta,
                NewValue = newValue,
                Multiplier = multiplier,
                IsSet = isSet,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Get mutation history for debugging.
        /// </summary>
        public IReadOnlyList<MutationRecord> GetMutationLog()
        {
            return _mutationLog?.AsReadOnly() ?? new List<MutationRecord>().AsReadOnly();
        }

        /// <summary>
        /// Get trace log showing full pipeline of modifications.
        /// </summary>
        public IReadOnlyList<TraceRecord> GetTraceLog()
        {
            return _traceLog?.AsReadOnly() ?? new List<TraceRecord>().AsReadOnly();
        }

        /// <summary>
        /// Get formatted trace string for debugging.
        /// </summary>
        public string GetTraceString()
        {
            if (_traceLog == null || _traceLog.Count == 0)
            {
                return $"BaseValue = {BaseValue} (no modifications)";
            }

            StringBuilder sb = new();
            sb.AppendLine($"BaseValue = {BaseValue}");

            foreach (TraceRecord? record in _traceLog)
            {
                string operation = record.IsSet ? "→" :
                                   !Mathf.Approximately(record.Multiplier, 1f) ? $"x{record.Multiplier} →" :
                                   record.Delta >= 0 ? $"+{record.Delta} →" : $"{record.Delta} →";

                sb.AppendLine($"{record.Source,-25} {operation} {record.NewValue}");
            }

            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Records a single mutation for debugging.
    /// </summary>
    public sealed class MutationRecord
    {
        public string Source { get; set; } = "";
        public float Delta { get; set; }
        public float NewValue { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Source}: {Delta:+#.##;-#.##;0} → {NewValue}";
        }
    }

    /// <summary>
    /// Records a single step in the value modification pipeline.
    /// </summary>
    public sealed class TraceRecord
    {
        public string Source { get; set; } = "";
        public float OldValue { get; set; }
        public float Delta { get; set; }
        public float NewValue { get; set; }
        public float Multiplier { get; set; } = 1f;
        public bool IsSet { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            if (IsSet)
                return $"{Source}: {OldValue} → {NewValue}";

            if (!Mathf.Approximately(Multiplier, 1f))
                return $"{Source}: {OldValue} x{Multiplier} → {NewValue}";

            return $"{Source}: {OldValue} {Delta:+#;-#;0} → {NewValue}";
        }
    }
}