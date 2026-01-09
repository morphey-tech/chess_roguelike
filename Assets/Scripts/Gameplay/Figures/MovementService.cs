using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    public class MovementService
    {
        private readonly BoardGrid _grid;

        public MovementService(BoardGrid grid)
        {
            _grid = grid;
        }

        public void MoveFigure(
            GridPosition from,
            GridPosition to)
        {
            BoardCell fromCell = _grid.GetBoardCell(from);
            BoardCell toCell = _grid.GetBoardCell(to);

            if (!toCell.IsWalkable)
            {
                return;
            }

            Figure figure = fromCell.OccupiedBy;
            fromCell.RemoveFigure();
            toCell.PlaceFigure(figure);
            toCell.Effects.OnEnter(toCell);
        }
    }
}