using Project.Core.Core.Grid;

namespace Project.Core.Core.ShrinkingZone.Core
{
    /// <summary>
    /// Статус клетки в shrinking zone
    /// </summary>
    public enum CellStatus
    {
        Safe = 0,      // Безопасная клетка
        Warning = 1,   // Предупреждение (скоро станет опасной)
        Danger = 2     // Опасная клетка (наносит урон)
    }

    /// <summary>
    /// Состояние системы shrinking zone
    /// </summary>
    public enum ZoneState
    {
        Inactive,      // Зона ещё не активирована
        Active,        // Зона активна
        MinSizeReached // Достигнут минимальный размер
    }

    /// <summary>
    /// Контекст для расчёта зоны
    /// </summary>
    public struct ZoneContext
    {
        public int BoardSize;
        public int CurrentLayer;
        public int StepInLayer;
        public int ShrinkInterval;
        public int SafeZoneMinSize;

        public ZoneContext(
            int boardSize,
            int currentLayer,
            int stepInLayer,
            int shrinkInterval,
            int safeZoneMinSize)
        {
            BoardSize = boardSize;
            CurrentLayer = currentLayer;
            StepInLayer = stepInLayer;
            ShrinkInterval = shrinkInterval;
            SafeZoneMinSize = safeZoneMinSize;
        }
    }

    /// <summary>
    /// Результат оценки клетки для AI
    /// </summary>
    public struct ZoneCellEvaluation
    {
        public GridPosition Position;
        public CellStatus Status;
        public int Cost;  // Стоимость для AI (Safe=0, Warning=50, Danger=1000)

        public ZoneCellEvaluation(GridPosition position, CellStatus status, int cost)
        {
            Position = position;
            Status = status;
            Cost = cost;
        }
    }
}
