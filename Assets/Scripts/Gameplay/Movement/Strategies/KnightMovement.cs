using System.Collections.Generic;
using System;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

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

        public IEnumerable<GridPosition> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in Offsets)
            {
                GridPosition to = new(from.Row + dr, from.Column + dc);
                
                if (grid.IsInside(to) && CanOccupy(figure, grid.GetBoardCell(to)))
                    yield return to;
            }
        }

        public bool CanMove(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            int dr = Math.Abs(to.Row - from.Row);
            int dc = Math.Abs(to.Column - from.Column);

            bool isLShape = (dr == 2 && dc == 1) || (dr == 1 && dc == 2);

            if (!isLShape)
            {
                return false;
            }

            return grid.IsInside(to) && CanOccupy(figure, grid.GetBoardCell(to));
        }

        private static bool CanOccupy(Figure figure, BoardCell cell)
        {
            return cell.IsFree || (cell.OccupiedBy != null && cell.OccupiedBy.Team != figure.Team);
        }
    }
}
