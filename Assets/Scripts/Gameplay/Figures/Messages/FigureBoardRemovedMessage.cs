namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Published when a figure is removed from the board (death, unplace, etc.).
    /// </summary>
    public readonly struct FigureBoardRemovedMessage
    {
        public int FigureId { get; }
        public Team Team { get; }

        public FigureBoardRemovedMessage(int figureId, Team team)
        {
            FigureId = figureId;
            Team = team;
        }
    }
}
