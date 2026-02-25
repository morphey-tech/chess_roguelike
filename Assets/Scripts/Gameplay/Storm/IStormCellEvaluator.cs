using Project.Core.Core.Grid;
using Project.Core.Core.Storm.Core;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Интерфейс оценщика клеток зоны для AI
    /// </summary>
    public interface IStormCellEvaluator
    {
        /// <summary>
        /// Оценить клетку для AI
        /// Возвращает стоимость: Safe=0, Warning=50, Danger=1000
        /// </summary>
        int EvaluateCell(int row, int col);

        /// <summary>
        /// Получить оценку для позиции
        /// </summary>
        StormCellEvaluation Evaluate(GridPosition position);
    }
}