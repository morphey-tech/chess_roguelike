using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Threat;

namespace Project.Gameplay.Gameplay.Stage.Analysis
{
    /// <summary>
    /// Результат анализа тактической ситуации на поле.
    /// </summary>
    public sealed class StageAnalysisResult
    {
        public ThreatMap EnemyThreatMap { get; }

        public StageAnalysisResult(ThreatMap enemyThreatMap)
        {
            EnemyThreatMap = enemyThreatMap;
        }

        /// <summary>
        /// Проверить, находится ли клетка под угрозой атаки врага.
        /// </summary>
        public bool IsCellDangerous(GridPosition pos)
        {
            return EnemyThreatMap.IsThreatened(pos);
        }

        /// <summary>
        /// Проверить, безопасно ли ходить на клетку.
        /// </summary>
        public bool IsCellSafe(GridPosition pos)
        {
            return !EnemyThreatMap.IsThreatened(pos);
        }
    }
}
