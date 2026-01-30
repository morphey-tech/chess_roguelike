using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    public sealed class BonusMoveController : IBonusMoveController
    {
        private readonly MovementService _movementService;
        private readonly IFigurePresenter _figurePresenter;
        private readonly ILogger<BonusMoveController> _logger;

        private Figure _actor;
        private GridPosition _from;
        private int _maxDistance;

        public bool IsActive => _actor != null;
        public Figure Actor => _actor;
        public GridPosition From => _from;

        [Inject]
        public BonusMoveController(
            MovementService movementService,
            IFigurePresenter figurePresenter,
            ILogService logService)
        {
            _movementService = movementService;
            _figurePresenter = figurePresenter;
            _logger = logService.CreateLogger<BonusMoveController>();
        }

        public void Start(Figure actor, GridPosition from, int maxDistance)
        {
            _actor = actor;
            _from = from;
            _maxDistance = maxDistance;
            
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

            // Validate move is legal
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
            if (!IsActive)
                yield break;

            foreach (GridPosition move in _movementService.GetAvailableMoves(_actor, _from))
            {
                int distance = Math.Abs(move.Row - _from.Row) + Math.Abs(move.Column - _from.Column);
                if (distance <= _maxDistance)
                {
                    yield return move;
                }
            }
        }

        private void Clear()
        {
            _actor = null;
            _from = default;
            _maxDistance = 0;
        }
    }
}
