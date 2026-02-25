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
}
