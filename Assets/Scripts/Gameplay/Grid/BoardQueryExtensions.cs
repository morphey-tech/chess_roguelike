using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Grid
{
    public static class BoardQueryExtensions
    {
        private static readonly Vector2Int[] Neighbours =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1),
            new(1, 1),
            new(1, -1),
            new(-1, 1),
            new(-1, -1),
        };

        public static int CountAlliesAround(
            this BoardGrid grid,
            Figure center)
        {
            BoardCell? figureCell = grid.FindFigure(center);

            if (figureCell == null)
            {
                return 0;
            }

            GridPosition pos = figureCell.Position;
            int count = 0;

            for (int index = 0; index < Neighbours.Length; index++)
            {
                Vector2Int dir = Neighbours[index];
                GridPosition neighbourPos = new(
                    pos.Row + dir.y,
                    pos.Column + dir.x
                );

                BoardCell cell = grid.GetBoardCell(neighbourPos);
                if (cell.IsFree)
                {
                    continue;
                }

                Figure? other = cell.OccupiedBy;
                if (other == null)
                {
                    continue;
                }

                if (other.Team == center.Team && !other.Stats.IsDead)
                {
                    count++;
                }
            }

            return count;
        }
    }
}