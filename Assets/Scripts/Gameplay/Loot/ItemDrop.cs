namespace Project.Gameplay.Gameplay.Loot
{
    public sealed class ItemDrop
    {
        public string ConfigId { get; }

        public ItemDrop(string configId)
        {
            ConfigId = configId;
        }
    }
}