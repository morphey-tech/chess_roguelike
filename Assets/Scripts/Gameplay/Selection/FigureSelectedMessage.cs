using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Selection
{
    public readonly struct FigureSelectedMessage
    {
        public readonly Figure Figure;
        public readonly GridPosition Position;

        public FigureSelectedMessage(Figure figure, GridPosition position)
        {
            Figure = figure;
            Position = position;
        }
    }
}