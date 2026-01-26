using System.Collections.Generic;
using System;
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

        private static readonly (int row, int col)[] Offsets =
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
                   continue;
                
                var cell = grid.GetBoardCell(to);
                var result = new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
                if (result.CanOccupy())
                    yield return result;
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            int dr = Math.Abs(to.Row - from.Row);
            int dc = Math.Abs(to.Column - from.Column);

            bool isLShape = (dr == 2 && dc == 1) || (dr == 1 && dc == 2);

            if (!isLShape)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            
            if(!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }
    }
}
