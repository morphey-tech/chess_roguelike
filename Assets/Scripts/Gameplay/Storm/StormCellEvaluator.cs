using Project.Core.Core.Grid;
using Project.Core.Core.Storm.Core;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Реализация оценщика на основе ZoneShrinkSystem
    /// </summary>
    public class StormCellEvaluator : IStormCellEvaluator
    {
        private readonly IStormQueryService _zoneSystem;

        // Стоимость клеток для AI, в конфиг?
        private const int SafeCost = 0;
        private const int WarningCost = 50;
        private const int DangerCost = 1000;

        [Inject]
        private StormCellEvaluator(IStormQueryService zoneSystem)
        {
            _zoneSystem = zoneSystem;
        }

        public int EvaluateCell(int row, int col)
        {
            StormCellStatus status = _zoneSystem.GetCellStatus(row, col);
            return GetCostForStatus(status);
        }

        public StormCellEvaluation Evaluate(GridPosition position)
        {
            StormCellStatus status = _zoneSystem.GetCellStatus(position.Row, position.Column);
            int cost = GetCostForStatus(status);
            return new StormCellEvaluation(position, status, cost);
        }

        private static int GetCostForStatus(StormCellStatus status)
        {
            switch (status)
            {
                case StormCellStatus.Safe:
                    return SafeCost;
                case StormCellStatus.Warning:
                    return WarningCost;
                case StormCellStatus.Danger:
                    return DangerCost;
                default:
                    return SafeCost;
            }
        }
    }
}
