using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Passive
{
    [Serializable]
    public sealed class PassiveConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("params")]
        public Dictionary<string, object> Params { get; set; } = new();

        public T Get<T>(string key, T defaultValue = default)
        {
            if (Params == null || !Params.TryGetValue(key, out object value))
                return defaultValue;
            
            try
            {
                if (value is T typedValue)
                    return typedValue;
                
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public float GetFloat(string key, float defaultValue = 0f) => Get(key, defaultValue);
        public int GetInt(string key, int defaultValue = 0) => Get(key, defaultValue);
        public bool GetBool(string key, bool defaultValue = false) => Get(key, defaultValue);
    }
}
