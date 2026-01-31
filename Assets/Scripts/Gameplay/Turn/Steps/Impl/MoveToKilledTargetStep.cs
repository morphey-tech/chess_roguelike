using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class MoveToKilledTargetStep : ITurnStep
    {
        public string Id => "move_to_killed";

        private readonly MovementService _movementService;
        private readonly IFigurePresenter _figurePresenter;
        private readonly ILogger<MoveToKilledTargetStep> _logger;

        public MoveToKilledTargetStep(
            MovementService movementService, 
            IFigurePresenter figurePresenter,
            ILogService logService)
        {
            _movementService = movementService;
            _figurePresenter = figurePresenter;
            _logger = logService.CreateLogger<MoveToKilledTargetStep>();
        }

        public UniTask ExecuteAsync(ActionContext context)
        {
            _logger.Debug($"MoveToKilled check: LastAttackKilledTarget={context.LastAttackKilledTarget}");
            
            if (!context.LastAttackKilledTarget)
                return UniTask.CompletedTask;

            int distance = Attack.AttackUtils.GetDistance(context.From, context.To);
            if (distance != 1)
            {
                _logger.Debug($"MoveToKilled skipped: distance={distance} (must be 1)");
                return UniTask.CompletedTask;
            }

            // Safety check: only move if target cell is actually free
            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            if (!targetCell.IsFree)
            {
                _logger.Warning($"MoveToKilled blocked: target cell ({context.To.Row},{context.To.Column}) is still occupied by {targetCell.OccupiedBy}!");
                return UniTask.CompletedTask;
            }

            _movementService.MoveFigure(context.From, context.To);
            _figurePresenter.MoveFigure(context.Actor.Id, context.To);
            context.From = context.To;
            
            _logger.Debug($"MoveToKilled: {context.Actor} moved to ({context.To.Row},{context.To.Column})");

            return UniTask.CompletedTask;
        }
    }
}
