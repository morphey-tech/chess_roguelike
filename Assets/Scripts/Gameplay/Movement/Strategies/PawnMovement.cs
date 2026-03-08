using Project.Core.Core.Combat;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Movement.Extensions;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Pawn movement: 1 cell forward or sideways (NOT backward).
    /// Forward direction is based on team (Player moves to higher row, Enemy to lower row).
    /// In Unity: Row 0 is near camera (bottom), Row 7 is far (top).
    /// Player starts at bottom and moves UP the board (to higher row numbers).
    /// </summary>
    public sealed class PawnMovement : IMovementStrategy
    {
        public string Id => "pawn";

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            int forwardDr = figure.Team == Team.Player ? 1 : -1;

            (int, int)[] directions = {
                (forwardDr, 0),
                (0, -1),
                (0, 1)
            };

            foreach ((int dr, int dc) in directions)
            {
                GridPosition to = new(from.Row + dr, from.Column + dc);
                if (!grid.IsInside(to))
                {
                    continue;
                }

                BoardCell cell = grid.GetBoardCell(to);
                MovementStrategyResult result = new(figure, to, true, cell.OccupiedBy);
                if (result.CanOccupy())
                {
                    yield return result;
                }
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            int forwardDr = figure.Team == Team.Player ? 1 : -1;
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            bool isForward = dr == forwardDr && dc == 0;
            bool isSideways = dr == 0 && System.Math.Abs(dc) == 1;

            if (!isForward && !isSideways)
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            BoardCell cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }
    }
}
