using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Knight: L-shape (2+1 or 1+2 cells).
    /// </summary>
    public sealed class KnightMovement : IMovementStrategy
    {
        public string Id => "knight";

        private static readonly (int dr, int dc)[] Offsets =
        {
            (-2, -1), (-2, 1),
            (-1, -2), (-1, 2),
            ( 1, -2), ( 1, 2),
            ( 2, -1), ( 2, 1)
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in Offsets)
            {
                GridPosition to = new(from.Row + dr, from.Column + dc);

                if (!grid.IsInside(to))
                {
                    continue;
                }

                BoardCell cell = grid.GetBoardCell(to);
                MovementStrategyResult result = new(figure, to, true, cell.OccupiedBy);
                if (!result.CanOccupy())
                {
                    continue;
                }

                yield return result;
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            bool isLShape = Offsets.Any(d => d.dr == dr && d.dc == dc);

            if (!isLShape || !grid.IsInside(to))
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            BoardCell cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }
    }
}
