using System.Collections.Generic;
using Project.Core.Core.Grid;

namespace Project.Core.Core.ShrinkingZone.Core
{
    /// <summary>
    /// Стратегия сужения зоны
    /// </summary>
    public interface IZoneShrinkStrategy
    {
        /// <summary>
        /// Получить клетки в статусе Warning для текущего состояния
        /// </summary>
        IEnumerable<GridPosition> GetWarningCells(ZoneContext context);

        /// <summary>
        /// Получить клетки в статусе Danger для текущего состояния
        /// </summary>
        IEnumerable<GridPosition> GetDangerCells(ZoneContext context);

        /// <summary>
        /// Проверить, есть ли следующий шаг в текущем слое
        /// </summary>
        bool HasNextStep(ZoneContext context);

        /// <summary>
        /// Перейти к следующему шагу сужения
        /// Возвращает true, если переход выполнен, false если достигнут конец
        /// </summary>
        bool AdvanceStep(ref ZoneContext context);

        /// <summary>
        /// Получить максимальное количество шагов в слое
        /// </summary>
        int GetMaxStepsInLayer(ZoneContext context);
    }
}
