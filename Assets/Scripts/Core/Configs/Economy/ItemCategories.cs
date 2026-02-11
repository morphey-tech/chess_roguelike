namespace Project.Core.Core.Configs.Economy
{
    /// <summary>
    /// Well-known category ids. Use constants to avoid typos and magic strings.
    /// Extend with new categories (e.g. artifact_passive, cursed_relic) as needed.
    /// </summary>
    public static class ItemCategories
    {
        public const string Artifact = "artifact";
        public const string Consumable = "consumable";
        public const string Relic = "relic";
        public const string Quest = "quest";
    }
}
