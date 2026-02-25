using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Movement.Extensions;
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

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in "straight".GetDirections())
            {
                GridPosition to = new(from.Row + dr * 2, from.Column + dc * 2);
                if (!grid.IsInside(to))
                {
                    continue;
                }

                GridPosition intermediate = new(from.Row + dr, from.Column + dc);
                BoardCell intermediateCell = grid.GetBoardCell(intermediate);
                if (intermediateCell.OccupiedBy != null)
                {
                    continue;
                }

                BoardCell cell = grid.GetBoardCell(to);
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

            if (!(dr, dc).IsStraightMove() || (dr, dc).GetMovementDistance() != 2)
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            GridPosition intermediate = new(from.Row + (dr > 0 ? 1 : -1), 
                from.Column + (dc > 0 ? 1 : -1));
            BoardCell intermediateCell = grid.GetBoardCell(intermediate);
            if (intermediateCell.OccupiedBy != null)
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            BoardCell targetCell = grid.GetBoardCell(to);
            return targetCell.OccupiedBy != null 
                ? MovementStrategyResult.MakeUnreachable(figure, to, null) 
                : new MovementStrategyResult(figure, to, true, null);
        }
    }
}
