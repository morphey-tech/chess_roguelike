using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Splasher movement:
    /// - 1 cell in any direction (8 directions)
    /// - OR 2 cells forward (based on team)
    /// </summary>
    public sealed class SplasherMovement : IMovementStrategy
    {
        public string Id => "splasher";

        // All 8 directions for 1-cell move
        private static readonly (int row, int col)[] AllDirections =
        {
            (-1, -1), (-1, 0), (-1, 1),
            ( 0, -1),          ( 0, 1),
            ( 1, -1), ( 1, 0), ( 1, 1)
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            int forwardDr = figure.Team == Team.Player ? 1 : -1;

            // 1 cell in any direction
            foreach ((int dr, int dc) in AllDirections)
            {
                GridPosition to = new(from.Row + dr, from.Column + dc);
                if (!grid.IsInside(to))
                    continue;

                var cell = grid.GetBoardCell(to);
                var result = new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
                if (result.CanOccupy())
                    yield return result;
            }

            // 2 cells forward
            GridPosition forward2 = new(from.Row + forwardDr * 2, from.Column);
            if (grid.IsInside(forward2))
            {
                var cell = grid.GetBoardCell(forward2);
                var result = new MovementStrategyResult(figure, forward2, true, cell.OccupiedBy);
                if (result.CanOccupy())
                    yield return result;
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            int forwardDr = figure.Team == Team.Player ? 1 : -1;

            // Check if it's a valid move
            bool isValid = IsValidSplasherMove(dr, dc, forwardDr);

            if (!isValid)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Check if path is clear for 2-cell forward move
            if (Math.Abs(dr) == 2 && dc == 0)
            {
                if (!IsPathClear(from, to, grid, forwardDr))
                    return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }

        private bool IsValidSplasherMove(int dr, int dc, int forwardDr)
        {
            // 1 cell in any direction
            if (Math.Max(Math.Abs(dr), Math.Abs(dc)) == 1)
            {
                return true;
            }

            // 2 cells forward only
            if (dr == forwardDr * 2 && dc == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsPathClear(GridPosition from, GridPosition to, BoardGrid grid, int forwardDr)
        {
            // Check the cell between from and to
            GridPosition middle = new(from.Row + forwardDr, from.Column);
            
            if (!grid.IsInside(middle))
                return false;

            BoardCell cell = grid.GetBoardCell(middle);
            return cell.OccupiedBy == null;
        }
    }
}
