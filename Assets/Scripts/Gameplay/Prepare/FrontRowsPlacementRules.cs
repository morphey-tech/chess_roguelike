using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Placement rules: only first N rows allowed.
    /// </summary>
    public sealed class FrontRowsPlacementRules : IPreparePlacementRules
    {
        private readonly BoardGrid _grid;
        private readonly int _allowedRows;

        public FrontRowsPlacementRules(BoardGrid grid, int allowedRows)
        {
            _grid = grid;
            _allowedRows = allowedRows;
        }

        public bool CanPlace(GridPosition pos)
        {
            if (_grid == null)
            {
                return false;
            }

            if (!_grid.IsInside(pos))
            {
                return false;
            }

            if (pos.Row >= _allowedRows)
            {
                return false;
            }

            BoardCell cell = _grid.GetBoardCell(pos);
            return cell.IsFree;
        }
    }
}
