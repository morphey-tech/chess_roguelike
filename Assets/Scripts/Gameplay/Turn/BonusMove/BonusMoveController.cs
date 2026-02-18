using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    /// <summary>
    /// Passive controller for bonus move DOMAIN logic only.
    /// Does NOT subscribe to clicks - TurnController handles click forwarding.
    /// Does NOT handle visuals - TurnController plays animations via VisualPipeline.
    /// </summary>
    public sealed class BonusMoveController : IBonusMoveController
    {
        public Figure Actor { get; private set; }
        
        public bool IsActive => Actor != null;
        public GridPosition From => _from;
        
        private readonly MovementService _movementService;
        private readonly ILogger<BonusMoveController> _logger;

        private GridPosition _from;
        private int _maxDistance;
        private BoardGrid _grid;

        [Inject]
        private BonusMoveController(
            MovementService movementService,
            ILogService logService)
        {
            _movementService = movementService;
            _logger = logService.CreateLogger<BonusMoveController>();
            
            _logger.Info("BonusMoveController created (passive mode)");
        }

        public void Start(Figure actor, GridPosition from, int maxDistance, BoardGrid grid)
        {
            Actor = actor;
            _from = from;
            _maxDistance = maxDistance;
            _grid = grid;
            
            _logger.Info($"Bonus move started for {actor} from ({from.Row},{from.Column}), max distance: {maxDistance}");
        }

        /// <summary>
        /// Validates and executes domain logic for bonus move.
        /// Does NOT play visuals - caller (TurnController) handles that via VisualPipeline.
        /// </summary>
        /// <returns>True if move was valid and domain was updated.</returns>
        public bool TryExecute(GridPosition to)
        {
            if (!IsActive)
            {
                _logger.Warning("TryExecute called but bonus move is not active");
                return false;
            }

            int distance = Math.Abs(to.Row - _from.Row) + Math.Abs(to.Column - _from.Column);
            if (distance > _maxDistance)
            {
                _logger.Debug($"Bonus move rejected: distance {distance} > max {_maxDistance}");
                return false;
            }

            BoardCell targetCell = _grid.GetBoardCell(to);
            if (!targetCell.IsFree)
            {
                _logger.Debug($"Bonus move rejected: cell ({to.Row},{to.Column}) is occupied by {targetCell.OccupiedBy}");
                return false;
            }

            if (!_movementService.CanMove(Actor, _from, to))
            {
                _logger.Debug($"Bonus move rejected: invalid move to ({to.Row},{to.Column})");
                return false;
            }

            _movementService.MoveFigure(_from, to);
            Actor.MovedThisTurn = true;
            _logger.Info($"{Actor} bonus moved to ({to.Row},{to.Column})");

            Clear();
            return true;
        }

        public void Cancel()
        {
            if (IsActive)
            {
                Figure actor = Actor;
                _logger.Info($"Bonus move cancelled for {actor}");
                Clear();
            }
        }

        public IEnumerable<MovementStrategyResult> GetAvailablePositions()
        {
            if (!IsActive || _grid == null)
            {
                yield break;
            }

            foreach (MovementStrategyResult moveResult in _movementService.GetAvailableMoves(Actor, _from))
            {
                GridPosition pos = moveResult.Position;
                int distance = Math.Abs(pos.Row - _from.Row) + Math.Abs(pos.Column - _from.Column);
                if (distance <= _maxDistance)
                {
                    if (moveResult.IsFree)
                    {
                        yield return moveResult;
                    }
                }
            }
        }

        private void Clear()
        {
            Actor = null;
            _from = default;
            _maxDistance = 0;
            _grid = null;
        }
    }
}
