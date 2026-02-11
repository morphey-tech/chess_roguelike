using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Configs.Economy;

namespace Project.Gameplay.Gameplay.Economy
{
    /// <summary>
    /// Container for runtime Item instances. Capacity + optional rules (e.g. max artifacts, 1 relic).
    /// </summary>
    public sealed class Inventory
    {
        private readonly List<Item> _items = new();
        private readonly IReadOnlyList<IInventoryRule> _rules;

        public int Capacity { get; }

        public IReadOnlyList<Item> Items => _items;
        public int Count => _items.Count;

        public Inventory(int capacity = 999, IReadOnlyList<IInventoryRule>? rules = null)
        {
            Capacity = capacity > 0 ? capacity : 999;
            _rules = rules ?? Array.Empty<IInventoryRule>();
        }

        public bool CanAdd(Item item)
        {
            if (item == null) return false;

            foreach (IInventoryRule rule in _rules)
            {
                if (!rule.CanAdd(this, item))
                    return false;
            }

            if (_items.Count >= Capacity) return false;
            if (item.MaxStack > 1)
            {
                Item? existing = _items.FirstOrDefault(i => i.ConfigId == item.ConfigId && i.CanStack);
                if (existing != null) return true;
            }
            return _items.Count + 1 <= Capacity;
        }

        public void Add(Item item)
        {
            if (!CanAdd(item)) return;

            if (item.MaxStack > 1)
            {
                Item? existing = _items.FirstOrDefault(
                    i => i.ConfigId == item.ConfigId && i.CanStack);

                if (existing != null)
                {
                    existing.StackCount += item.StackCount;
                    if (existing.StackCount > existing.MaxStack)
                        existing.StackCount = existing.MaxStack;
                    return;
                }
            }

            _items.Add(item);
        }

        public bool Remove(string instanceId)
        {
            Item? item = Find(instanceId);
            if (item == null) return false;
            _items.Remove(item);
            return true;
        }

        public bool RemoveByConfigId(string configId)
        {
            Item? item = _items.FirstOrDefault(i => i.ConfigId == configId);
            if (item == null) return false;
            _items.Remove(item);
            return true;
        }

        public Item? Find(string instanceId)
        {
            return _items.FirstOrDefault(i => i.Id == instanceId);
        }

        public Item? FindByConfigId(string configId)
        {
            return _items.FirstOrDefault(i => i.ConfigId == configId);
        }

        public IReadOnlyList<Item> GetByCategory(string categoryId)
        {
            return _items.Where(i => i.CategoryId == categoryId).ToList();
        }

        public bool Contains(string configId)
        {
            return _items.Any(i => i.ConfigId == configId);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
