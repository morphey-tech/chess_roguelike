using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct PushVisualContext
    {
        public int FigureId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }

        public PushVisualContext(int figureId, GridPosition from, GridPosition to)
        {
            FigureId = figureId;
            From = from;
            To = to;
        }
    }
}
