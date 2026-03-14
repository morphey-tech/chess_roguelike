using Project.Core.Core.Combat;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    public readonly struct FigureBoardMessage
    {
        public const string SPAWNED = "figureSpawned";
        public const string REMOVED = "figureRemoved";
        public const string MOVED = "figureMoved";

        public readonly Figure Figure;
        public readonly GridPosition Position;
        public readonly GridPosition? FromPosition;

        public FigureBoardMessage(Figure figure, GridPosition position, GridPosition? fromPosition = null)
        {
            Figure = figure;
            Position = position;
            FromPosition = fromPosition;
        }
    }
}
