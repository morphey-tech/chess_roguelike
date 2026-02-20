using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
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
            // Forward direction based on team
            // Player moves to higher row (Z+ in Unity), Enemy to lower row
            int forwardDr = figure.Team == Team.Player ? 1 : -1;
            
            // Forward, left, right (NOT backward)
            var directions = new[]
            {
                (forwardDr, 0),      // Forward
                (0, -1),             // Left
                (0, 1)               // Right
            };

            foreach ((int dr, int dc) in directions)
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
            int forwardDr = figure.Team == Team.Player ? 1 : -1;
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            // Valid moves: forward (1 step), left/right (1 step)
            bool isForward = (dr == forwardDr && dc == 0);
            bool isSideways = (dr == 0 && Math.Abs(dc) == 1);

            if (!isForward && !isSideways)
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            if (!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }
    }
}
