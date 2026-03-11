using Project.Core.Core.Combat;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    public readonly struct FigureBoardMessage
    {
        public const string SPAWNED = "figureSpawned";
        public const string REMOVED = "figureRemoved";

        public readonly Figure Figure;
        public readonly GridPosition Position;

        public FigureBoardMessage(Figure figure, GridPosition position)
        {
            Figure = figure;
            Position = position;
        }
    }
}
