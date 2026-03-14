using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Stage
{
    public interface IStageQueryService
    {
        StageSelectionInfo GetSelectionInfo(Figure actor, GridPosition pos);
        IReadOnlyCollection<GridPosition> GetBonusMoveTargets();

        /// <summary>
        /// Получить клетки, находящиеся под атакой врага для данной фигуры.
        /// Устарело: используйте StageAnalysisService.AnalyzeActor() вместо этого.
        /// </summary>
        [System.Obsolete("Используйте StageAnalysisService.AnalyzeActor() для получения DangerousCells")]
        IReadOnlyCollection<GridPosition> GetUnderAttackCells(Figure actor, GridPosition pos);

        void Clear();
    }
}
