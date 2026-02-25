using Project.Core.Core.Grid;

namespace Project.Core.Core.Storm.Messages
{
    /// <summary>
    /// Сообщение об обновлении клеток зоны
    /// </summary>
    public readonly struct StormCellsUpdatedMessage
    {
        public readonly GridPosition[] WarningCells;
        public readonly GridPosition[] DangerCells;

        public StormCellsUpdatedMessage(GridPosition[] warningCells, GridPosition[] dangerCells)
        {
            WarningCells = warningCells;
            DangerCells = dangerCells;
        }
    }
}