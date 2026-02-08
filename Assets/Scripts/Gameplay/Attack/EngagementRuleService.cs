using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class EngagementRuleService : IEngagementRuleService
    {
        public bool IsEngaged(Figure unit, BoardGrid grid)
        {
            if (unit == null)
                return false;
            BoardCell cell = grid.FindFigure(unit);
            if (cell == null)
                return false;

            GridPosition from = cell.Position;
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0)
                        continue;
                    GridPosition pos = new(from.Row + dr, from.Column + dc);
                    if (!grid.IsInside(pos))
                        continue;
                    BoardCell adj = grid.GetBoardCell(pos);
                    if (adj.OccupiedBy != null && adj.OccupiedBy.Team != unit.Team)
                        return true;
                }
            }
            return false;
        }
    }
}
