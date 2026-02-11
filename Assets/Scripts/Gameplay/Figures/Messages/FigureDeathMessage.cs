namespace Project.Gameplay.Gameplay.Figures
{
    public readonly struct FigureDeathMessage
    {
        public int FigureId { get; }
        public Team Team { get; }
        public string? LootTableId { get; }
        /// <summary>True when death came from combat (KillEffect). Loot is then applied by LootPresenter; handler should skip.</summary>
        public bool FromCombat { get; }

        public FigureDeathMessage(int figureId, Team team, string? lootTableId = null, bool fromCombat = false)
        {
            FigureId = figureId;
            Team = team;
            LootTableId = lootTableId;
            FromCombat = fromCombat;
        }
    }
}
