using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Input.Messages
{
    public readonly struct CellClickedMessage
    {
        public readonly GridPosition Position;

        public CellClickedMessage(GridPosition position)
        {
            Position = position;
        }
    }
}
