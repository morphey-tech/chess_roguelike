using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Ranger movement: like Queen (straight + diagonal) but limited to 2 cells.
    /// </summary>
    public sealed class RangerMovement : IMovementStrategy
    {
        public string Id => "ranger";

        // All 8 directions (straight + diagonal)
        private static readonly (int row, int col)[] AllDirections =
        {
            (-1, -1), (-1, 0), (-1, 1),
            ( 0, -1),          ( 0, 1),
            ( 1, -1), ( 1, 0), ( 1, 1)
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in AllDirections)
            {
                for (int i = 1; i <= 2; i++)
                {
                    GridPosition to = new(from.Row + dr * i, from.Column + dc * i);
                    if (!grid.IsInside(to))
                        break;

                    var cell = grid.GetBoardCell(to);
                    var result = new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
                    if (!result.CanOccupy())
                        break;

                    yield return result;
                }
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            // Must be straight or diagonal
            if (!IsStraightOrDiagonal(dr, dc))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Must be 1 or 2 cells
            int distance = Math.Max(Math.Abs(dr), Math.Abs(dc));
            if (distance < 1 || distance > 2)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Check path is clear
            if (!IsPathClear(from, to, grid, dr, dc))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }

        private bool IsStraightOrDiagonal(int dr, int dc)
        {
            // Straight: dr == 0 or dc == 0
            // Diagonal: |dr| == |dc|
            return dr == 0 || dc == 0 || Math.Abs(dr) == Math.Abs(dc);
        }

        private bool IsPathClear(GridPosition from, GridPosition to, BoardGrid grid, int dr, int dc)
        {
            int stepR = dr == 0 ? 0 : (dr > 0 ? 1 : -1);
            int stepC = dc == 0 ? 0 : (dc > 0 ? 1 : -1);

            GridPosition current = new(from.Row + stepR, from.Column + stepC);

            while (current.Row != to.Row || current.Column != to.Column)
            {
                if (!grid.IsInside(current))
                    return false;

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null)
                    return false; // Blocked by a figure

                current = new(current.Row + stepR, current.Column + stepC);
            }

            return true;
        }
    }
}
