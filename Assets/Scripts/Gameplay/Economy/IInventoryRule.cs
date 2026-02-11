namespace Project.Gameplay.Gameplay.Economy
{
    /// <summary>
    /// Rule that can allow or deny adding an item to inventory.
    /// E.g. max artifacts, 1 relic, 3 key items — without rewriting Inventory.
    /// </summary>
    public interface IInventoryRule
    {
        bool CanAdd(Inventory inventory, Item item);
    }
}
