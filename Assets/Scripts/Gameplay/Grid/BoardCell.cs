using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Cells;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Grid
{
    public class BoardCell : IGridCell
    {
        public GridPosition Position { get; }

        public Figure? OccupiedBy { get; private set; }
        public CellEffectContainer Effects { get; }

        public bool IsFree => OccupiedBy == null;

        public BoardCell(GridPosition position)
        {
            Position = position;
            Effects = new CellEffectContainer();
        }

        public void PlaceFigure(Figure figure)
        {
            OccupiedBy = figure;
        }

        public void RemoveFigure()
        {
            OccupiedBy = null;
        }
    }
}