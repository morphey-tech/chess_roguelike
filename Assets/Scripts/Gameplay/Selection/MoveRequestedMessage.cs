using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Selection
{
    public readonly struct MoveRequestedMessage
    {
        public readonly GridPosition From;
        public readonly GridPosition To;

        public MoveRequestedMessage(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }
    }
}