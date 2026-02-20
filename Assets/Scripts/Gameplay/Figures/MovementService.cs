using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Movement;
using Project.Gameplay.Movement;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public class MovementService
    {
        public BoardGrid? Grid { get; private set; }

        private readonly MovementStrategyFactory _strategyFactory;
        private readonly PassiveTriggerService _passiveTriggerService;
        private readonly ILogger<MovementService> _logger;

        [Inject]
        private MovementService(
            MovementStrategyFactory strategyFactory,
            PassiveTriggerService passiveTriggerService,
            ILogService logService)
        {
            _strategyFactory = strategyFactory;
            _passiveTriggerService = passiveTriggerService;
            _logger = logService.CreateLogger<MovementService>();
        }

        public void Configure(BoardGrid grid)
        {
            Grid = grid;
            _logger.Debug($"Grid set: {grid.Width}x{grid.Height}");
        }

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from)
        {
            if (Grid == null)
            {
                _logger.Error("Grid not set!");
                yield break;
            }

            IMovementStrategy strategy = _strategyFactory.Get(figure.MovementId);
            foreach (MovementStrategyResult move in strategy.GetAvailableMoves(figure, from, Grid))
            {
                yield return move;
            }
        }

        public bool CanMove(GridPosition from, GridPosition to)
        {
            if (Grid == null)
            {
                _logger.Error("Grid not set!");
                return false;
            }

            if (!Grid.IsInside(from) || !Grid.IsInside(to))
                return false;

            BoardCell fromCell = Grid.GetBoardCell(from);
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
            if (Grid == null)
            {
                _logger.Error("Grid not set!");
                return false;
            }

            IMovementStrategy strategy = _strategyFactory.Get(figure.MovementId);
            MovementStrategyResult strategyResult = strategy.GetFor(figure, from, to, Grid);

            if (!strategyResult.CanOccupy())
            {
                _logger.Debug($"Move rejected by {strategy.Id} strategy");
            }

            return strategyResult.CanOccupy();
        }

        public void MoveFigure(GridPosition from, GridPosition to)
        {
            if (Grid == null)
            {
                _logger.Error("Grid not set!");
                return;
            }

            BoardCell fromCell = Grid.GetBoardCell(from);
            BoardCell toCell = Grid.GetBoardCell(to);
            Figure figure = fromCell.OccupiedBy;
            if (figure == null)
            {
                _logger.Warning($"No figure at ({from.Row},{from.Column})");
                return;
            }

            // Сохраняем предыдущую позицию для пассивок
            figure.PreviousPosition = from;

            fromCell.RemoveFigure();
            toCell.PlaceFigure(figure);
            toCell.Effects.OnEnter(toCell);

            // Set moved flag
            figure.MovedThisTurn = true;

            // Вызываем триггеры движения
            var moveContext = new MoveContext
            {
                Actor = figure,
                From = from,
                To = to,
                Grid = Grid,
                CurrentTurn = 0, // TODO: получить текущий ход из TurnService
                DidMove = true
            };
            _passiveTriggerService.TriggerMove(figure, moveContext);

            _logger.Info($"Moved {figure} from ({from.Row},{from.Column}) to ({to.Row},{to.Column})");
        }
    }
}
