using System;
using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    public sealed class BonusMoveController : IBonusMoveController, IDisposable
    {
        private readonly MovementService _movementService;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<BonusMoveCompletedMessage> _completedPublisher;
        private readonly ILogger<BonusMoveController> _logger;
        private readonly IDisposable _subscriptions;

        private Figure _actor;
        private GridPosition _from;
        private int _maxDistance;
        private BoardGrid _grid;

        public bool IsActive => _actor != null;
        public Figure Actor => _actor;
        public GridPosition From => _from;

        [Inject]
        public BonusMoveController(
            MovementService movementService,
            IFigurePresenter figurePresenter,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            IPublisher<BonusMoveCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _movementService = movementService;
            _figurePresenter = figurePresenter;
            _completedPublisher = completedPublisher;
            _logger = logService.CreateLogger<BonusMoveController>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            if (!IsActive)
                return;

            if (_grid == null || !_grid.IsInside(message.Position))
                return;

            _logger.Debug($"Bonus move click: ({message.Position.Row},{message.Position.Column})");
            
            if (TryExecute(message.Position))
            {
                _completedPublisher.Publish(new BonusMoveCompletedMessage(_actor));
            }
        }

        public void Start(Figure actor, GridPosition from, int maxDistance, BoardGrid grid)
        {
            _actor = actor;
            _from = from;
            _maxDistance = maxDistance;
            _grid = grid;
            
            _logger.Info($"Bonus move started for {actor} from ({from.Row},{from.Column}), max distance: {maxDistance}");
        }

        public bool TryExecute(GridPosition to)
        {
            if (!IsActive)
            {
                _logger.Warning("TryExecute called but bonus move is not active");
                return false;
            }

            // Validate distance
            int distance = Math.Abs(to.Row - _from.Row) + Math.Abs(to.Column - _from.Column);
            if (distance > _maxDistance)
            {
                _logger.Debug($"Bonus move rejected: distance {distance} > max {_maxDistance}");
                return false;
            }

            // CRITICAL: Bonus move is movement only - target cell must be FREE
            // (no attacks allowed during bonus move)
            BoardCell targetCell = _grid.GetBoardCell(to);
            if (!targetCell.IsFree)
            {
                _logger.Debug($"Bonus move rejected: cell ({to.Row},{to.Column}) is occupied by {targetCell.OccupiedBy}");
                return false;
            }

            // Validate move is legal (path, etc.)
            if (!_movementService.CanMove(_actor, _from, to))
            {
                _logger.Debug($"Bonus move rejected: invalid move to ({to.Row},{to.Column})");
                return false;
            }

            // Execute the move
            _movementService.MoveFigure(_from, to);
            _figurePresenter.MoveFigure(_actor.Id, to);
            
            _logger.Info($"{_actor} bonus moved to ({to.Row},{to.Column})");
            
            Clear();
            return true;
        }

        public void Cancel()
        {
            if (IsActive)
            {
                _logger.Info($"Bonus move cancelled for {_actor}");
                Clear();
            }
        }

        public IEnumerable<GridPosition> GetAvailablePositions()
        {
            if (!IsActive || _grid == null)
                yield break;

            foreach (GridPosition move in _movementService.GetAvailableMoves(_actor, _from))
            {
                int distance = Math.Abs(move.Row - _from.Row) + Math.Abs(move.Column - _from.Column);
                if (distance <= _maxDistance)
                {
                    // Only free cells - no attacks during bonus move
                    BoardCell cell = _grid.GetBoardCell(move);
                    if (cell.IsFree)
                    {
                        yield return move;
                    }
                }
            }
        }

        private void Clear()
        {
            _actor = null;
            _from = default;
            _maxDistance = 0;
            _grid = null;
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
