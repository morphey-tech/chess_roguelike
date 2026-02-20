using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Grid
{
    public static class BoardQueryExtensions
    {
        public static int CountAlliesAround(
            this BoardGrid grid,
            Figure center)
        {
            BoardCell? figureCell = grid.FindFigure(center);
            if (figureCell == null)
                return 0;

            int count = 0;
            foreach (BoardCell cell in grid.GetAdjacentCells(figureCell.Position))
            {
                Figure? other = cell.OccupiedBy;
                if (other != null && other.Team == center.Team && !other.Stats.IsDead)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Returns all figures within radius (Manhattan distance) from center position.
        /// </summary>
        public static IEnumerable<Figure> GetFiguresInRadius(
            this BoardGrid grid,
            GridPosition center,
            int radius)
        {
            for (int dr = -radius; dr <= radius; dr++)
            {
                for (int dc = -radius; dc <= radius; dc++)
                {
                    if (dr == 0 && dc == 0)
                        continue;

                    GridPosition pos = new(center.Row + dr, center.Column + dc);
                    if (!grid.IsInside(pos))
                        continue;

                    var cell = grid.GetBoardCell(pos);
                    if (cell.OccupiedBy != null)
                        yield return cell.OccupiedBy;
                }
            }
        }
    }
}