namespace Project.Core.Core.Configs.Economy
{
    /// <summary>
    /// When/where the item exists: run (per run), meta (persistent), temporary (e.g. curse, rental).
    /// </summary>
    public enum ItemLifetime
    {
        Run,
        Meta,
        Temporary
    }
}
