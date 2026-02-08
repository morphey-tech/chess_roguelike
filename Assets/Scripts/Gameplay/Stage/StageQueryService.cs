using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Movement;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageQueryService : IStageQueryService
    {
        private readonly MovementService _movementService;
        private readonly IAttackQueryService _attackQuery;
        private readonly IBonusMoveController _bonusMoveController;
        private readonly ILogger<StageQueryService> _logger;

        [Inject]
        public StageQueryService(
            MovementService movementService,
            IAttackQueryService attackQuery,
            IBonusMoveController bonusMoveController,
            ILogService logService)
        {
            _movementService = movementService;
            _attackQuery = attackQuery;
            _bonusMoveController = bonusMoveController;
            _logger = logService.CreateLogger<StageQueryService>();
        }

        public StageSelectionInfo GetSelectionInfo(Figure? actor, GridPosition pos)
        {
            if (actor == null)
                return new StageSelectionInfo(null, null);

            var moveTargets = new HashSet<GridPosition>();
            foreach (MovementStrategyResult move in _movementService.GetAvailableMoves(actor, pos))
            {
                if (move.CanOccupy() && move.IsFree)
                    moveTargets.Add(move.Position);
            }

            var attackTargets = _attackQuery.GetTargets(actor, pos, _movementService.Grid);

            return new StageSelectionInfo(moveTargets, attackTargets);
        }

        public IReadOnlyCollection<GridPosition> GetBonusMoveTargets()
        {
            var result = new HashSet<GridPosition>();
            foreach (MovementStrategyResult move in _bonusMoveController.GetAvailablePositions())
            {
                if (move.CanOccupy() && move.IsFree)
                    result.Add(move.Position);
            }
            return result;
        }

        public void Clear()
        {
            // no cached state
        }
    }
}
