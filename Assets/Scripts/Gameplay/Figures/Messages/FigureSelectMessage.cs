using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    public struct FigureSelectMessage
    {
        public const string SELECTED = "figureSelected";
        public const string DESELECTED = "figureDeselected";

        public readonly Figure Figure;
        public readonly GridPosition Position;

        public FigureSelectMessage(Figure figure, GridPosition position)
        {
            Figure = figure;
            Position = position;
        }
    }
}