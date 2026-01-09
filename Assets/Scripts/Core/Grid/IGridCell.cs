namespace Project.Core.Core.Grid
{
    public interface IGridCell
    {
        GridPosition Position { get; }
        bool IsWalkable { get; }
    }
}