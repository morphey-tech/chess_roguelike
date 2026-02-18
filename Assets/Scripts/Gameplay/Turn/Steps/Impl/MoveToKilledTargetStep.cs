using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    /// <summary>
    /// Moves attacker to killed target's position (melee only).
    /// 
    /// PIPELINE:
    /// 1. Domain: MovementService updates grid state
    /// 2. Visual: VisualPipeline plays move animation
    /// </summary>
    public sealed class MoveToKilledTargetStep : ITurnStep
    {
        public string Id => "move_to_killed";

        private readonly MovementService _movementService;
        private readonly VisualPipeline _visualPipeline;
        private readonly ILogger<MoveToKilledTargetStep> _logger;

        public MoveToKilledTargetStep(
            MovementService movementService, 
            VisualPipeline visualPipeline,
            ILogService logService)
        {
            _movementService = movementService;
            _visualPipeline = visualPipeline;
            _logger = logService.CreateLogger<MoveToKilledTargetStep>();
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            _logger.Debug($"MoveToKilled check: LastAttackKilledTarget={context.LastAttackKilledTarget}");
            
            if (!context.LastAttackKilledTarget)
                return;

            int distance = Attack.AttackUtils.GetDistance(context.From, context.To);
            if (distance != 1)
            {
                _logger.Debug($"MoveToKilled skipped: distance={distance} (must be 1)");
                return;
            }

            // Safety check: only move if target cell is actually free
            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            if (!targetCell.IsFree)
            {
                _logger.Warning($"MoveToKilled blocked: target cell ({context.To.Row},{context.To.Column}) is still occupied by {targetCell.OccupiedBy}!");
                return;
            }

            // === DOMAIN ===
            _movementService.MoveFigure(context.From, context.To);
            context.Actor.MovedThisTurn = true;

            // === VISUAL ===
            using (VisualScope scope = _visualPipeline.BeginScope())
            {
                scope.Enqueue(new MoveCommand(new MoveVisualContext(context.Actor.Id, context.To)));
                await scope.PlayAsync();
            }

            context.From = context.To;
            _logger.Debug($"MoveToKilled: {context.Actor} moved to ({context.To.Row},{context.To.Column})");
        }
    }
}
