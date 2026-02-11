namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct DeathVisualContext
    {
        public int FigureId { get; }
        public string? DeathReason { get; }

        public DeathVisualContext(int figureId, string? deathReason = null)
        {
            FigureId = figureId;
            DeathReason = deathReason;
        }
    }
}