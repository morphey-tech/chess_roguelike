using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Slippery movement: 
    /// - Up to 2 cells in straight directions (forward/backward/left/right)
    /// - OR 1 cell diagonally
    /// </summary>
    public sealed class SlipperyMovement : IMovementStrategy
    {
        public string Id => "slippery";

        // Straight directions (cross pattern)
        private static readonly (int row, int col)[] StraightDirections =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        // Diagonal directions
        private static readonly (int row, int col)[] DiagonalDirections =
        {
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            // Straight moves (up to 2 cells)
            foreach ((int dr, int dc) in StraightDirections)
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

            // Diagonal moves (1 cell only)
            foreach ((int dr, int dc) in DiagonalDirections)
            {
                GridPosition to = new(from.Row + dr, from.Column + dc);
                if (!grid.IsInside(to))
                    continue;

                var cell = grid.GetBoardCell(to);
                var result = new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
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

            // Check if it's a valid move
            bool isValid = IsValidSlipperyMove(dr, dc);

            if (!isValid)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Check if path is clear for straight moves
            if (IsStraightMove(dr, dc))
            {
                if (!IsPathClear(from, to, grid))
                    return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }

        private bool IsValidSlipperyMove(int dr, int dc)
        {
            // Straight move (cross pattern): up to 2 cells
            if (dr == 0 || dc == 0)
            {
                return Math.Max(Math.Abs(dr), Math.Abs(dc)) <= 2;
            }

            // Diagonal move: exactly 1 cell
            if (Math.Abs(dr) == 1 && Math.Abs(dc) == 1)
            {
                return true;
            }

            return false;
        }

        private bool IsStraightMove(int dr, int dc)
        {
            return dr == 0 || dc == 0;
        }

        private bool IsPathClear(GridPosition from, GridPosition to, BoardGrid grid)
        {
            int dr = Math.Sign(to.Row - from.Row);
            int dc = Math.Sign(to.Column - from.Column);

            GridPosition current = new(from.Row + dr, from.Column + dc);

            while (current.Row != to.Row || current.Column != to.Column)
            {
                if (!grid.IsInside(current))
                    return false;

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null)
                    return false; // Blocked by a figure

                current = new(current.Row + dr, current.Column + dc);
            }

            return true;
        }
    }
}
