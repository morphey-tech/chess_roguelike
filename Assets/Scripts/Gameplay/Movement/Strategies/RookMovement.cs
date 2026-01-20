using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Rook: straight lines (horizontal/vertical), unlimited distance.
    /// </summary>
    public sealed class RookMovement : IMovementStrategy
    {
        public string Id => "rook";

        private static readonly (int row, int col)[] Directions =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        public IEnumerable<GridPosition> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in Directions)
            {
                for (int i = 1; i < 20; i++)
                {
                    GridPosition to = new(from.Row + dr * i, from.Column + dc * i);

                    if (!grid.IsInside(to))
                        break;

                    BoardCell cell = grid.GetBoardCell(to);
                    
                    if (cell.IsFree)
                    {
                        yield return to;
                    }
                    else
                    {
                        // Can capture enemy, then stop
                        if (cell.OccupiedBy?.Team != figure.Team)
                            yield return to;
                        break;
                    }
                }
            }
        }

        public bool CanMove(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            if (dr != 0 && dc != 0)
                return false;
            if (dr == 0 && dc == 0)
                return false;

            int stepR = dr == 0 ? 0 : (dr > 0 ? 1 : -1);
            int stepC = dc == 0 ? 0 : (dc > 0 ? 1 : -1);

            // Check path is clear (excluding destination)
            GridPosition current = new(from.Row + stepR, from.Column + stepC);
            while (current.Row != to.Row || current.Column != to.Column)
            {
                if (!grid.IsInside(current) || !grid.GetBoardCell(current).IsFree)
                    return false;
                current = new GridPosition(current.Row + stepR, current.Column + stepC);
            }

            return grid.IsInside(to) && CanOccupy(figure, grid.GetBoardCell(to));
        }

        private static bool CanOccupy(Figure figure, BoardCell cell)
        {
            return cell.IsFree || (cell.OccupiedBy != null && cell.OccupiedBy.Team != figure.Team);
        }
    }
}
