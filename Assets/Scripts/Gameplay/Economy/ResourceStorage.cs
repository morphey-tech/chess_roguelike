using System.Collections.Generic;
using Project.Core.Core.Configs.Economy;
using UniRx;

namespace Project.Gameplay.Gameplay.Economy
{
    /// <summary>
    /// Dictionary-based storage for accumulative resources (gold, crystals, keys, etc.).
    /// One instance per persistence scope. Main thread only — no locks.
    /// </summary>
    public sealed class ResourceStorage
    {
        private readonly Dictionary<string, ReactiveProperty<int>> _data = new();

        public int Get(string id)
        {
            return _data.TryGetValue(id, out var prop) ? prop.Value : 0;
        }

        public IReadOnlyReactiveProperty<int> GetProperty(string id)
        {
            if (!_data.ContainsKey(id))
            {
                _data[id] = new ReactiveProperty<int>(0);
            }
            return _data[id];
        }

        public void Add(string id, int value)
        {
            if (value <= 0)
            {
                return;
            }

            if (!_data.ContainsKey(id))
            {
                _data[id] = new ReactiveProperty<int>(0);
            }

            _data[id].Value += value;
        }

        public bool TrySpend(string id, int value)
        {
            if (value <= 0)
            {
                return true;
            }

            if (!_data.ContainsKey(id))
            {
                return false;
            }

            int current = _data[id].Value;
            if (current < value)
            {
                return false;
            }

            _data[id].Value -= value;
            return true;
        }

        /// <summary>
        /// Check and spend multiple resources at once (shop, exchange, quest).
        /// </summary>
        public bool TrySpend(ResourceCost cost)
        {
            if (cost?.Costs == null || cost.Costs.Count == 0)
            {
                return true;
            }
            foreach (KeyValuePair<string, int> kvp in cost.Costs)
            {
                if (Get(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }
            foreach (KeyValuePair<string, int> kvp in cost.Costs)
            {
                TrySpend(kvp.Key, kvp.Value);
            }
            return true;
        }

        /// <summary>
        /// Spend multi-resource cost. Call only after TrySpend(cost) returns true.
        /// </summary>
        public void Spend(ResourceCost cost)
        {
            if (cost?.Costs == null)
            {
                return;
            }
            foreach (KeyValuePair<string, int> kvp in cost.Costs)
            {
                TrySpend(kvp.Key, kvp.Value);
            }
        }

        public void Set(string id, int value)
        {
            if (!_data.ContainsKey(id))
            {
                _data[id] = new ReactiveProperty<int>(0);
            }
            _data[id].Value = value;
        }

        public bool Has(string id, int amount = 1)
        {
            return Get(id) >= amount;
        }

        public bool Has(ResourceCost cost)
        {
            if (cost?.Costs == null)
            {
                return true;
            }
            foreach (KeyValuePair<string, int> kvp in cost.Costs)
            {
                if (Get(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public Dictionary<string, int> GetAll()
        {
            var result = new Dictionary<string, int>();
            foreach (var kvp in _data)
            {
                result[kvp.Key] = kvp.Value.Value;
            }
            return result;
        }

        public void Load(Dictionary<string, int>? data)
        {
            _data.Clear();
            if (data == null)
            {
                return;
            }

            foreach (KeyValuePair<string, int> kvp in data)
            {
                _data[kvp.Key] = new ReactiveProperty<int>(kvp.Value);
            }
        }

        public void Clear()
        {
            _data.Clear();
        }
    }
}
