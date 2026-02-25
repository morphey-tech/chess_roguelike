namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Расширения для AI pathfinding с учётом зоны
    /// </summary>
    public static class StormPathfindingExtensions
    {
        /// <summary>
        /// Добавить штраф зоны к стоимости пути
        /// </summary>
        public static int AddCost(this int baseCost, IStormCellEvaluator evaluator, int row, int col)
        {
            return baseCost + evaluator.EvaluateCell(row, col);
        }

        /// <summary>
        /// Проверить, безопасна ли клетка для перемещения
        /// </summary>
        public static bool IsSafeForMove(this IStormCellEvaluator evaluator, int row, int col, int maxAcceptableCost = 100)
        {
            return evaluator.EvaluateCell(row, col) <= maxAcceptableCost;
        }
    }
}