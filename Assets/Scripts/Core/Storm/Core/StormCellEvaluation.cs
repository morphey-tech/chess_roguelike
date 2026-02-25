using Project.Core.Core.Grid;

namespace Project.Core.Core.Storm.Core
{
    /// <summary>
    /// Результат оценки клетки для AI
    /// </summary>
    public struct StormCellEvaluation
    {
        public GridPosition Position;
        public StormCellStatus Status;
        public int Cost;  // Стоимость для AI (Safe=0, Warning=50, Danger=1000)

        public StormCellEvaluation(GridPosition position, StormCellStatus status, int cost)
        {
            Position = position;
            Status = status;
            Cost = cost;
        }
    }
}
