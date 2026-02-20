using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Cells;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Grid
{
    public class BoardCell : Entity, IGridCell
    {
        public GridPosition Position { get; }

        private Figure? _occupiedBy;
        public Figure? OccupiedBy => _occupiedBy;

        public CellEffectContainer Effects { get; }

        public bool IsFree => _occupiedBy == null;

        public BoardCell(int id, GridPosition position) : base(id)
        {
            Position = position;
            Effects = new CellEffectContainer();
        }

        public void PlaceFigure(Figure figure)
        {
            _occupiedBy = figure;
        }

        public void RemoveFigure()
        {
            _occupiedBy = null;
        }

        /// <summary>
        /// Internal method for BoardGrid to set occupant.
        /// Use BoardGrid.PlaceFigure/RemoveFigure instead.
        /// </summary>
        internal void SetOccupant(Figure? figure)
        {
            _occupiedBy = figure;
        }
    }
}