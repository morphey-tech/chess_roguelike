using Project.Core.Core.Combat;

namespace Project.Gameplay.Gameplay.Figures
{
    public readonly struct FigureDiedMessage
    {
        public int FigureId { get; }
        public Team Team { get; }
        public string? LootTableId { get; }
        public bool FromCombat { get; }

        public FigureDiedMessage(int figureId, Team team,
            string? lootTableId = null, bool fromCombat = false)
        {
            FigureId = figureId;
            Team = team;
            LootTableId = lootTableId;
            FromCombat = fromCombat;
        }
    }
}
