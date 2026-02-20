using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Tank movement: 1 cell in straight directions (forward, backward, left, right) - no diagonals.
    /// Cross pattern, 1 cell.
    /// </summary>
    public sealed class TankMovement : IMovementStrategy
    {
        public string Id => "tank";

        // Straight directions (cross pattern)
        private static readonly (int row, int col)[] StraightDirections =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in StraightDirections)
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

            // Must be straight line (no diagonals)
            if (!IsStraightMove(dr, dc))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Must be exactly 1 cell
            if (Math.Max(Math.Abs(dr), Math.Abs(dc)) != 1)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }

        private bool IsStraightMove(int dr, int dc)
        {
            return dr == 0 || dc == 0;
        }
    }
}
