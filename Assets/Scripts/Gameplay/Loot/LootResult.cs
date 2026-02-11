using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Loot
{
    /// <summary>
    /// Pure result of a loot roll. No side effects.
    /// Applied to economy by LootPresenter after visual (or immediately by apply-only presenter).
    /// </summary>
    public sealed class LootResult
    {
        public List<ResourceDrop> Resources { get; } = new();
        public List<ItemDrop> Items { get; } = new();

        public bool IsEmpty => Resources.Count == 0 && Items.Count == 0;
    }

    public sealed class ResourceDrop
    {
        public string Id { get; }
        public int Amount { get; }

        public ResourceDrop(string id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }

    public sealed class ItemDrop
    {
        public string ConfigId { get; }

        public ItemDrop(string configId)
        {
            ConfigId = configId;
        }
    }
}
