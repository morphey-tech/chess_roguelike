using System;
using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Turn.Conditions
{
    public sealed class ConditionParams
    {
        public static readonly ConditionParams Empty = new();
        
        private readonly Dictionary<string, object> _values;

        public ConditionParams()
        {
            _values = new Dictionary<string, object>();
        }

        public ConditionParams(Dictionary<string, object> values)
        {
            _values = values ?? new Dictionary<string, object>();
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_values.TryGetValue(key, out object value))
            {
                return value switch
                {
                    int i => i,
                    long l => (int)l,
                    double d => (int)d,
                    float f => (int)f,
                    _ => defaultValue
                };
            }
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (_values.TryGetValue(key, out object value))
            {
                return value switch
                {
                    float f => f,
                    double d => (float)d,
                    int i => i,
                    long l => l,
                    _ => defaultValue
                };
            }
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_values.TryGetValue(key, out object value))
            {
                return value switch
                {
                    bool b => b,
                    int i => i != 0,
                    _ => defaultValue
                };
            }
            return defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (_values.TryGetValue(key, out object value))
                return value?.ToString() ?? defaultValue;
            return defaultValue;
        }

        public ConditionParams MergeWith(ConditionParams overrides)
        {
            if (overrides == null || overrides._values.Count == 0)
                return this;

            var merged = new Dictionary<string, object>(_values);
            foreach (var kvp in overrides._values)
                merged[kvp.Key] = kvp.Value;

            return new ConditionParams(merged);
        }
    }
}
