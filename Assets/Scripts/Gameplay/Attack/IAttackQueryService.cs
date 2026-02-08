using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public interface IAttackQueryService
    {
        IReadOnlyCollection<GridPosition> GetTargets(Figure actor, GridPosition from, BoardGrid grid);
    }
}
