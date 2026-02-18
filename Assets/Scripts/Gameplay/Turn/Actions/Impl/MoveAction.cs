using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Turn.Actions.Impl
{
    /// <summary>
    /// Executes move action.
    /// 
    /// PIPELINE:
    /// 1. Domain: MovementService updates grid state
    /// 2. Visual: VisualPipeline plays move animation
    /// </summary>
    public sealed class MoveAction : ICombatAction
    {
        public string Id { get; }

        private readonly MovementService _movementService;
        private readonly VisualPipeline _visualPipeline;

        public MoveAction(string id, MovementService movementService, VisualPipeline visualPipeline)
        {
            Id = id;
            _movementService = movementService;
            _visualPipeline = visualPipeline;
        }

        public bool CanExecute(ActionContext context)
        {
            return _movementService.CanMove(context.From, context.To);
        }

        public IReadOnlyCollection<GridPosition> GetValidTargets(Figure actor, GridPosition from, BoardGrid grid)
        {
            var targets = new HashSet<GridPosition>();
            foreach (MovementStrategyResult move in _movementService.GetAvailableMoves(actor, from))
            {
                if (move.CanOccupy() && move.IsFree)
                    targets.Add(move.Position);
            }
            return targets;
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            if (!CanExecute(context))
                return;

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
        }
    }
}
