using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.Actions;
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
        private readonly TurnPatternResolver _patternResolver;
        private readonly ILogger<StageQueryService> _logger;

        [Inject]
        public StageQueryService(
            MovementService movementService,
            IAttackQueryService attackQuery,
            IBonusMoveController bonusMoveController,
            TurnPatternResolver patternResolver,
            ILogService logService)
        {
            _movementService = movementService;
            _attackQuery = attackQuery;
            _bonusMoveController = bonusMoveController;
            _patternResolver = patternResolver;
            _logger = logService.CreateLogger<StageQueryService>();
        }

        public StageSelectionInfo GetSelectionInfo(Figure? actor, GridPosition pos)
        {
            if (actor == null || actor.TurnPattern == null)
                return new StageSelectionInfo(null, null);

            // Use actions from TurnPattern to get valid targets (single source of truth)
            var moveTargets = new HashSet<GridPosition>();
            var attackTargets = new HashSet<GridPosition>();

            // Build context for pattern resolution
            Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;
            var enemies = _movementService.Grid.GetFiguresByTeam(enemyTeam).ToList();
            var context = new ActionContext
            {
                Actor = actor,
                Grid = _movementService.Grid,
                From = pos,
                To = pos, // Will be set per action
                Enemies = enemies,
                MovementService = _movementService,
                ActionExecuted = false
            };

            BoardGrid grid = _movementService.Grid;
            foreach (var patternDesc in actor.TurnPattern.Patterns)
            {
                var action = patternDesc.Action;

                var previews = action.GetPreviews(actor, pos, grid);

                foreach (var preview in previews)
                {
                    if (preview.MoveTo.HasValue)
                    {
                        moveTargets.Add(preview.MoveTo.Value);
                    }
                    if (preview.Target != null)
                    {
                        BoardCell targetCell = grid.FindFigure(preview.Target)!;
                        attackTargets.Add(targetCell.Position);
                    }
                }
            }
            
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
