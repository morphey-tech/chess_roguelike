namespace Project.Core.Core.Grid
{
    public interface IGridCell
    {
        int Id { get; }
        GridPosition Position { get; }
        bool IsFree { get; }
    }
}