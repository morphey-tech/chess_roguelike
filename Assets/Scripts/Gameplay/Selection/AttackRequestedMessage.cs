using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Selection
{
    public readonly struct AttackRequestedMessage
    {
        public readonly GridPosition From;
        public readonly GridPosition To;

        public AttackRequestedMessage(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }
    }
}
