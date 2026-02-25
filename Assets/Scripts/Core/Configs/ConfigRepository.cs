using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs
{
    public abstract class ConfigRepository<T> where T : class
    {
        public IReadOnlyList<T> All => Items ?? Array.Empty<T>();
        
        protected abstract IReadOnlyList<T>? Items { get; }
        protected abstract string GetKey(T item);
        
        [JsonIgnore]
        private Dictionary<string, T?>? _byKey;

        public bool Contains(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            EnsureIndex();
            return _byKey!.ContainsKey(key);
        }

        public T? Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            EnsureIndex();
            _byKey!.TryGetValue(key, out T? value);
            return value;
        }

        public T Require(string key)
        {
            T? value = Get(key);
            return value ?? throw new Exception($"{typeof(T).Name} config not found: {key}");
        }

        protected void ResetIndex()
        {
            _byKey = null;
        }

        private void EnsureIndex()
        {
            if (_byKey != null)
            {
                return;
            }
            Dictionary<string, T> dict = new Dictionary<string, T>(StringComparer.Ordinal);
            if (Items != null)
            {
                foreach (T item in Items)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    string key = GetKey(item);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }
                    dict[key] = item;
                }
            }
            _byKey = dict!;
        }
    }
}
