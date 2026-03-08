using System;
using System.Collections.Generic;
using Project.Core.Core.Combat;
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

        /// <summary>
        /// Checks if the straight-line path between two positions is clear of obstacles.
        /// Works for horizontal, vertical, and diagonal lines.
        /// </summary>
        public static bool IsPathClear(
            this BoardGrid grid,
            GridPosition from,
            GridPosition to)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            int stepR = dr == 0 ? 0 : Math.Sign(dr);
            int stepC = dc == 0 ? 0 : Math.Sign(dc);

            GridPosition current = new(from.Row + stepR, from.Column + stepC);

            while (current.Row != to.Row || current.Column != to.Column)
            {
                if (!grid.IsInside(current))
                    return false;

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null)
                    return false;

                current = new(current.Row + stepR, current.Column + stepC);
            }

            return true;
        }

        /// <summary>
        /// Returns adjacent cells in the specified direction offsets.
        /// </summary>
        public static IEnumerable<BoardCell> GetAdjacentCells(
            this BoardGrid grid,
            GridPosition position,
            params (int dr, int dc)[] directions)
        {
            foreach ((int dr, int dc) in directions)
            {
                GridPosition neighbor = new(position.Row + dr, position.Column + dc);
                if (grid.IsInside(neighbor))
                {
                    yield return grid.GetBoardCell(neighbor);
                }
            }
        }

        /// <summary>
        /// Returns enemy figures in adjacent cells in the specified direction offsets.
        /// </summary>
        public static IEnumerable<Figure> GetAdjacentEnemies(
            this BoardGrid grid,
            GridPosition position,
            Team enemyTeam,
            params (int dr, int dc)[] directions)
        {
            foreach ((int dr, int dc) in directions)
            {
                GridPosition neighbor = new(position.Row + dr, position.Column + dc);
                if (!grid.IsInside(neighbor))
                    continue;

                BoardCell cell = grid.GetBoardCell(neighbor);
                if (cell.OccupiedBy != null &&
                    cell.OccupiedBy.Team == enemyTeam)
                {
                    yield return cell.OccupiedBy;
                }
            }
        }
    }
}