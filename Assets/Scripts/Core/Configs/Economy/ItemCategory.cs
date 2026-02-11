namespace Project.Core.Core.Configs.Economy
{
    /// <summary>
    /// Category of item as string id. Extensible without enum explosion
    /// (e.g. artifact_passive, artifact_active, cursed_relic, temporary_relic).
    /// </summary>
    public sealed class ItemCategory
    {
        public string Id { get; }

        public ItemCategory(string id)
        {
            Id = id ?? string.Empty;
        }

        public override string ToString() => Id;
    }
}
