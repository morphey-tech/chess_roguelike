using Project.Core.Core.Grid;
using Project.Core.Core.ShrinkingZone.Core;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Интерфейс оценщика клеток зоны для AI
    /// </summary>
    public interface IZoneCellEvaluator
    {
        /// <summary>
        /// Оценить клетку для AI
        /// Возвращает стоимость: Safe=0, Warning=50, Danger=1000
        /// </summary>
        int EvaluateCell(int row, int col);

        /// <summary>
        /// Получить оценку для позиции
        /// </summary>
        ZoneCellEvaluation Evaluate(GridPosition position);
    }

    /// <summary>
    /// Реализация оценщика на основе ZoneShrinkSystem
    /// </summary>
    public class ZoneCellEvaluator : IZoneCellEvaluator
    {
        private readonly IZoneShrinkQueryService _zoneSystem;

        // Стоимость клеток для AI
        private const int SafeCost = 0;
        private const int WarningCost = 50;
        private const int DangerCost = 1000;

        public ZoneCellEvaluator(IZoneShrinkQueryService zoneSystem)
        {
            _zoneSystem = zoneSystem;
        }

        public int EvaluateCell(int row, int col)
        {
            var status = _zoneSystem.GetCellStatus(row, col);
            return GetCostForStatus(status);
        }

        public ZoneCellEvaluation Evaluate(GridPosition position)
        {
            var status = _zoneSystem.GetCellStatus(position.Row, position.Column);
            int cost = GetCostForStatus(status);
            
            return new ZoneCellEvaluation(position, status, cost);
        }

        private int GetCostForStatus(CellStatus status)
        {
            switch (status)
            {
                case CellStatus.Safe:
                    return SafeCost;
                case CellStatus.Warning:
                    return WarningCost;
                case CellStatus.Danger:
                    return DangerCost;
                default:
                    return SafeCost;
            }
        }
    }

    /// <summary>
    /// Расширения для AI pathfinding с учётом зоны
    /// </summary>
    public static class ZonePathfindingExtensions
    {
        /// <summary>
        /// Добавить штраф зоны к стоимости пути
        /// </summary>
        public static int AddZoneCost(this int baseCost, IZoneCellEvaluator evaluator, int row, int col)
        {
            return baseCost + evaluator.EvaluateCell(row, col);
        }

        /// <summary>
        /// Проверить, безопасна ли клетка для перемещения
        /// </summary>
        public static bool IsSafeForMove(this IZoneCellEvaluator evaluator, int row, int col, int maxAcceptableCost = 100)
        {
            return evaluator.EvaluateCell(row, col) <= maxAcceptableCost;
        }
    }
}
