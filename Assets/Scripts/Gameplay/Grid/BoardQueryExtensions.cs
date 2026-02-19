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
    }
}