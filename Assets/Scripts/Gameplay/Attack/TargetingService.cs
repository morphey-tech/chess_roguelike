using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class TargetingService : ITargetingService
    {
        public bool CanTarget(GridPosition from, GridPosition to, AttackProfile attack,
            BoardGrid grid, Team attackerTeam)
        {
            int distance = AttackUtils.GetDistance(from, to);
            if (!attack.CanHit(distance))
                return false;

            return attack.Targeting switch
            {
                TargetingType.Adjacent => distance <= 1,
                TargetingType.StraightLine => IsStraightLine(from, to) && HasClearPathIgnoringAllies(from, to, grid, attackerTeam),
                TargetingType.AnyLine => IsStraightOrDiagonal(from, to) && HasClearPathIgnoringAllies(from, to, grid, attackerTeam),
                TargetingType.Arc => true,
                TargetingType.Area => true,
                _ => false
            };
        }

        private static bool IsStraightLine(GridPosition from, GridPosition to)
        {
            return from.Row == to.Row || from.Column == to.Column;
        }

        private static bool IsStraightOrDiagonal(GridPosition from, GridPosition to)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;
            return dr == 0 || dc == 0 || System.Math.Abs(dr) == System.Math.Abs(dc);
        }

        private static bool HasClearPathIgnoringAllies(GridPosition from, GridPosition to,
            BoardGrid grid, Team attackerTeam)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;
            int stepR = System.Math.Sign(dr);
            int stepC = System.Math.Sign(dc);
            int steps = System.Math.Max(System.Math.Abs(dr), System.Math.Abs(dc));

            // exclude target cell
            for (int i = 1; i < steps; i++)
            {
                GridPosition pos = new(from.Row + stepR * i, from.Column + stepC * i);
                if (!grid.IsInside(pos))
                    return false;
                BoardCell cell = grid.GetBoardCell(pos);
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != attackerTeam)
                    return false;
            }
            return true;
        }
    }
}
