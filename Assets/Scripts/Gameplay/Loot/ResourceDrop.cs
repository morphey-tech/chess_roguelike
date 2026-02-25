namespace Project.Gameplay.Gameplay.Loot
{
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
}