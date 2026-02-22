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
        private readonly IBonusMoveController _bonusMoveController;

        [Inject]
        private StageQueryService(
            MovementService movementService,
            IAttackQueryService attackQuery,
            IBonusMoveController bonusMoveController,
            TurnPatternResolver patternResolver,
            ILogService logService)
        {
            _movementService = movementService;
            _bonusMoveController = bonusMoveController;
        }

        public StageSelectionInfo GetSelectionInfo(Figure? actor, GridPosition pos)
        {
            if (actor?.TurnPattern == null)
            {
                return new StageSelectionInfo(null, null);
            }

            HashSet<GridPosition> moveTargets = new HashSet<GridPosition>();
            HashSet<GridPosition> attackTargets = new HashSet<GridPosition>();

            Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;
            List<Figure> enemies = _movementService.Grid.GetFiguresByTeam(enemyTeam).ToList();

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
