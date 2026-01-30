using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Movement;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public class MovementService
    {
        public BoardGrid Grid => _grid;
        
        private readonly MovementStrategyFactory _strategyFactory;
        private readonly ILogger<MovementService> _logger;
        private BoardGrid _grid;

        [Inject]
        private MovementService(
            MovementStrategyFactory strategyFactory,
            ILogService logService)
        {
            _strategyFactory = strategyFactory;
            _logger = logService.CreateLogger<MovementService>();
        }

        public void Configure(BoardGrid grid)
        {
            _grid = grid;
            _logger.Debug($"Grid set: {grid.Width}x{grid.Height}");
        }

        public IEnumerable<GridPosition> GetAvailableMoves(Figure figure, GridPosition from)
        {
            if (_grid == null)
            {
                _logger.Error("Grid not set!");
                yield break;
            }

            IMovementStrategy strategy = _strategyFactory.Get(figure.MovementId);
            foreach (GridPosition move in strategy.GetAvailableMoves(figure, from, _grid))
            {
                yield return move;
            }
        }

        public bool CanMove(GridPosition from, GridPosition to)
        {
            if (_grid == null)
            {
                _logger.Error("Grid not set!");
                return false;
            }

            if (!_grid.IsInside(from) || !_grid.IsInside(to))
                return false;

            BoardCell fromCell = _grid.GetBoardCell(from);
            Figure figure = fromCell.OccupiedBy;

            if (figure == null)
            {
                _logger.Warning($"No figure at ({from.Row},{from.Column})");
                return false;
            }

            return CanMove(figure, from, to);
        }

        public bool CanMove(Figure figure, GridPosition from, GridPosition to)
        {
            if (_grid == null)
            {
                _logger.Error("Grid not set!");
                return false;
            }

            IMovementStrategy strategy = _strategyFactory.Get(figure.MovementId);
            bool canMove = strategy.CanMove(figure, from, to, _grid);

            if (!canMove)
            {
                _logger.Debug($"Move rejected by {strategy.Id} strategy");
            }

            return canMove;
        }

        public void MoveFigure(GridPosition from, GridPosition to)
        {
            if (_grid == null)
            {
                _logger.Error("Grid not set!");
                return;
            }

            BoardCell fromCell = _grid.GetBoardCell(from);
            BoardCell toCell = _grid.GetBoardCell(to);
            Figure figure = fromCell.OccupiedBy;
            if (figure == null)
            {
                _logger.Warning($"No figure at ({from.Row},{from.Column})");
                return;
            }

            fromCell.RemoveFigure();
            toCell.PlaceFigure(figure);
            toCell.Effects.OnEnter(toCell);

            _logger.Info($"Moved {figure} from ({from.Row},{from.Column}) to ({to.Row},{to.Column})");
        }
    }
}
