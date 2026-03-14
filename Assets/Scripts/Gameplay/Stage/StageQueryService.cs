using Project.Core.Core.Combat;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Combat.Threat;
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
        private readonly IBonusMoveController _bonusMoveController;
        private readonly IAttackQueryService _attackQueryService;
        private readonly ThreatMapService _threatMapService;
        private readonly ILogger<StageQueryService> _logger;

        [Inject]
        private StageQueryService(
            MovementService movementService,
            IAttackQueryService attackQueryService,
            ThreatMapService threatMapService,
            IBonusMoveController bonusMoveController,
            TurnPatternResolver patternResolver,
            ILogService logService)
        {
            _movementService = movementService;
            _attackQueryService = attackQueryService;
            _threatMapService = threatMapService;
            _bonusMoveController = bonusMoveController;
            _logger = logService.CreateLogger<StageQueryService>();
        }

        public StageSelectionInfo GetSelectionInfo(Figure? actor, GridPosition pos)
        {
            if (actor?.TurnPattern == null)
            {
                _logger.Warning($"GetSelectionInfo: {actor?.Id} has no TurnPattern");
                return new StageSelectionInfo(null, null);
            }

            HashSet<GridPosition> moveTargets = new();
            HashSet<GridPosition> attackTargets = new();

            BoardGrid grid = _movementService.Grid;
            foreach (TurnPatternDescription? patternDesc in actor.TurnPattern.Patterns)
            {
                ICombatAction action = patternDesc.Action;
                IReadOnlyCollection<ActionPreview> previews = action.GetPreviews(actor, pos, grid);

                foreach (ActionPreview? preview in previews)
                {
                    if (preview.MoveTo.HasValue)
                    {
                        moveTargets.Add(preview.MoveTo.Value);
                    }
                    // Используем AttackPosition вместо Target для надёжности
                    if (preview.AttackPosition.HasValue)
                    {
                        attackTargets.Add(preview.AttackPosition.Value);
                    }
                    else if (preview.Target != null)
                    {
                        BoardCell targetCell = grid.FindFigure(preview.Target)!;
                        attackTargets.Add(targetCell.Position);
                    }
                }
            }

            _logger.Debug($"GetSelectionInfo: {actor.Id} moves={moveTargets.Count} attacks={attackTargets.Count}");
            return new StageSelectionInfo(moveTargets, attackTargets);
        }

        public IReadOnlyCollection<GridPosition> GetBonusMoveTargets()
        {
            HashSet<GridPosition> result = new();
            foreach (MovementStrategyResult move in _bonusMoveController.GetAvailablePositions())
            {
                if (move.CanOccupy() && move.IsFree)
                {
                    result.Add(move.Position);
                }
            }
            return result;
        }

        public IReadOnlyCollection<GridPosition> GetUnderAttackCells(Figure actor, GridPosition pos)
        {
            HashSet<GridPosition> result = new();

            Team enemyTeam = actor.Team == Team.Player
                ? Team.Enemy
                : Team.Player;

            ThreatMap threatMap = _threatMapService.GetThreatMap(enemyTeam);

            StageSelectionInfo selectionInfo = GetSelectionInfo(actor, pos);

            foreach (GridPosition move in selectionInfo.MoveTargets)
            {
                if (threatMap.IsThreatened(move))
                {
                    result.Add(move);
                }
            }

            if (threatMap.IsThreatened(pos))
            {
                result.Add(pos);
            }

            return result;
        }

        public void Clear()
        {
            // no cached state
        }
    }
}
