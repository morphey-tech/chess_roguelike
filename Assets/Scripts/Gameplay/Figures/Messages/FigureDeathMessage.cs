namespace Project.Gameplay.Gameplay.Figures
{
    public readonly struct FigureDeathMessage
    {
        public int FigureId { get; }
        public Team Team { get; }

        public FigureDeathMessage(int figureId, Team team)
        {
            FigureId = figureId;
            Team = team;
        }
    }
}
