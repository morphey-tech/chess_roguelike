using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Pawn: 1 cell forward (direction based on team).
    /// For roguelike - can move in any direction 1 cell.
    /// </summary>
    public sealed class PawnMovement : IMovementStrategy
    {
        public string Id => "pawn";

        private static readonly (int row, int col)[] Directions =
        {
                      (-1, 0),
            ( 0, -1),          ( 0, 1),
                      ( 1, 0),
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in Directions)
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
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            if (!((Math.Abs(dr) == 1 && dc == 0) || (Math.Abs(dc) == 1 && dr == 0)))
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }
            
            if(!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }
    }
}
