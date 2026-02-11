using System;
using System.Collections.Generic;
using Project.Core.Core.Configs.Economy;
using Project.Gameplay.Gameplay.Combat;
using static Project.Core.Core.Configs.Economy.ItemCategories;

namespace Project.Gameplay.Gameplay.Economy
{
    /// <summary>
    /// Runtime item instance. Created from ItemConfig via ItemFactory.
    /// Config defines the template; this is a live instance with unique Id and stack count.
    /// </summary>
    public sealed class Item
    {
        public string Id { get; }
        public string ConfigId { get; }
        public string CategoryId { get; }
        public ItemLifetime Lifetime { get; }
        public string Name { get; }
        public int MaxStack { get; }
        public int StackCount { get; set; }
        public IReadOnlyList<IPassive> Passives { get; }
        public ItemParams Params { get; }

        public Item(
            string id,
            string configId,
            string categoryId,
            ItemLifetime lifetime,
            string name,
            int maxStack,
            IReadOnlyList<IPassive> passives,
            ItemParams? @params = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ConfigId = configId ?? throw new ArgumentNullException(nameof(configId));
            CategoryId = categoryId ?? Artifact;
            Lifetime = lifetime;
            Name = name ?? configId;
            MaxStack = maxStack;
            StackCount = 1;
            Passives = passives ?? Array.Empty<IPassive>();
            Params = @params ?? new ItemParams();
        }

        public bool CanStack => StackCount < MaxStack;

        public override string ToString() => $"{Name} ({ConfigId}) x{StackCount}";
    }
}
