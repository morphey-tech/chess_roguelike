namespace Project.Core.Core.ShrinkingZone.Core
{
    /// <summary>
    /// Интерфейс для query-методов shrinking zone
    /// </summary>
    public interface IZoneShrinkQueryService
    {
        /// <summary>
        /// Получить статус клетки
        /// </summary>
        CellStatus GetCellStatus(int row, int col);

        /// <summary>
        /// Получить текущее состояние системы
        /// </summary>
        ZoneState GetCurrentState();

        /// <summary>
        /// Получить текущий ход активации
        /// </summary>
        int GetActivationTurn();
    }

    /// <summary>
    /// Интерфейс цели, которая может получать урон от зоны
    /// </summary>
    public interface IZoneDamageTarget
    {
        /// <summary>
        /// Максимальное здоровье
        /// </summary>
        int MaxHP { get; }

        /// <summary>
        /// Получить урон
        /// </summary>
        void TakeDamage(int damage);
    }
}
