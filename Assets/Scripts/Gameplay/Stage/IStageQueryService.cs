using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Stage
{
    public interface IStageQueryService
    {
        StageSelectionInfo GetSelectionInfo(Figure actor, GridPosition pos);
        IReadOnlyCollection<GridPosition> GetBonusMoveTargets();
        IReadOnlyCollection<GridPosition> GetUnderAttackCells(Figure actor, GridPosition pos);
        void Clear();
    }
}
