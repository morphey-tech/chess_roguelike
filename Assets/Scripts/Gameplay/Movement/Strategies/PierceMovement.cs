using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Piercer movement: exactly 2 cells in straight directions (forward, backward, left, right).
    /// Must jump over exactly one cell (can be occupied or empty).
    /// </summary>
    public sealed class PierceMovement : IMovementStrategy
    {
        public string Id => "pierce";

        // Straight directions (cross pattern)
        private static readonly (int row, int col)[] StraightDirections =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in StraightDirections)
            {
                // Move exactly 2 cells
                GridPosition to = new(from.Row + dr * 2, from.Column + dc * 2);
                if (!grid.IsInside(to))
                    continue;

                // Check intermediate cell is empty (no jumping)
                GridPosition intermediate = new(from.Row + dr, from.Column + dc);
                var intermediateCell = grid.GetBoardCell(intermediate);
                if (intermediateCell.OccupiedBy != null)
                    continue; // Blocked

                var cell = grid.GetBoardCell(to);
                
                // Target cell must be empty
                if (cell.OccupiedBy == null)
                {
                    yield return new MovementStrategyResult(figure, to, true, null);
                }
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            // Must be straight line
            if (!IsStraightMove(dr, dc))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Must be exactly 2 cells
            if (Math.Max(Math.Abs(dr), Math.Abs(dc)) != 2)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Check intermediate cell is empty (no jumping)
            GridPosition intermediate = new(from.Row + (dr > 0 ? 1 : -1), from.Column + (dc > 0 ? 1 : -1));
            var intermediateCell = grid.GetBoardCell(intermediate);
            if (intermediateCell.OccupiedBy != null)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Target cell must be empty
            var targetCell = grid.GetBoardCell(to);
            if (targetCell.OccupiedBy != null)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            return new MovementStrategyResult(figure, to, true, null);
        }

        private bool IsStraightMove(int dr, int dc)
        {
            return dr == 0 || dc == 0;
        }
    }
}
