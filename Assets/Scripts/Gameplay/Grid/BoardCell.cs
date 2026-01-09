using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Grid
{
    public class BoardCell : IGridCell
    {
        public GridPosition Position { get; }

        //TODO: temp
        public bool IsWalkable => false;
        
        public BoardCell(GridPosition position)
        {
            Position = position;
        }
    }
}