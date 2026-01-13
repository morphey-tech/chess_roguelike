using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Pure gameplay message - no Unity dependencies.
    /// </summary>
    public readonly struct FigureSpawnedMessage
    {
        public readonly Figure Figure;
        public readonly GridPosition Position;

        public FigureSpawnedMessage(Figure figure, GridPosition position)
        {
            Figure = figure;
            Position = position;
        }
    }
}
