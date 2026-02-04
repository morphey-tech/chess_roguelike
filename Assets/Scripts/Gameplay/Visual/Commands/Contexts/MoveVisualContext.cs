using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct MoveVisualContext
    {
        public int FigureId { get; }
        public GridPosition To { get; }

        public MoveVisualContext(int figureId, GridPosition to)
        {
            FigureId = figureId;
            To = to;
        }
    }
}
