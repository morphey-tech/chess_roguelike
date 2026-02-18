using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Turn.Actions.Impl
{
    /// <summary>
    /// Moves attacker to killed target's position (melee only).
    /// Only executes if LastAttackKilledTarget is true and target is adjacent.
    /// </summary>
    public sealed class MoveToKilledTargetAction : ICombatAction
    {
        public string Id => "move_to_killed";

        private readonly MovementService _movementService;
        private readonly VisualPipeline _visualPipeline;
        private readonly ILogger<MoveToKilledTargetAction> _logger;

        public MoveToKilledTargetAction(
            MovementService movementService,
            VisualPipeline visualPipeline,
            ILogService logService)
        {
            _movementService = movementService;
            _visualPipeline = visualPipeline;
            _logger = logService.CreateLogger<MoveToKilledTargetAction>();
        }

        public bool CanExecute(ActionContext context)
        {
            if (!context.LastAttackKilledTarget)
                return false;

            int distance = Attack.AttackUtils.GetDistance(context.From, context.To);
            if (distance != 1)
                return false;

            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            return targetCell.IsFree;
        }

        public IReadOnlyCollection<ActionPreview> GetPreviews(Figure actor, GridPosition from, BoardGrid grid)
        {
            // This action is only valid after a kill, so we can't predict targets upfront.
            // Return empty set - the action will be evaluated by condition (LastAttackKilledTarget).
            return new HashSet<ActionPreview>();
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            if (!CanExecute(context))
                return;

            _logger.Debug($"MoveToKilled: {context.Actor} moving to killed target");

            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            if (!targetCell.IsFree)
            {
                _logger.Warning($"MoveToKilled blocked: target cell ({context.To.Row},{context.To.Column}) is still occupied!");
                return;
            }

            // === DOMAIN ===
            _movementService.MoveFigure(context.From, context.To);
            context.Actor.MovedThisTurn = true;
            context.ActionExecuted = true;

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
