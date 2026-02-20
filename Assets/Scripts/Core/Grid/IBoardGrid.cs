namespace Project.Core.Core.Grid
{
    public interface IBoardGrid
    {
        bool IsInside(GridPosition position);
        IGridCell GetCell(GridPosition position);
    }
}
