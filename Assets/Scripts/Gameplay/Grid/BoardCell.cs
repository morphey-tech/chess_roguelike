using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Cells;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Grid
{
    public class BoardCell : Entity, IGridCell
    {
        public GridPosition Position { get; }
        public Figure? OccupiedBy { get; private set; }
        public CellEffectContainer Effects { get; }

        public IFigureView? OccupiedBy2 { get; private set; }
        public bool IsFree => OccupiedBy == null && OccupiedBy2 == null;

        public BoardCell(int id, GridPosition position) : base(id)
        {
            Position = position;
            Effects = new CellEffectContainer();
        }

        public void PlaceFigure(Figure figure)
        {
            OccupiedBy = figure;
        }

        public void PlaceFigure(IFigureView figure)
        {
            OccupiedBy2 = figure;
        }
        
        public void RemoveFigure()
        {
            OccupiedBy = null;
            OccupiedBy2 = null;
        }
    }
}