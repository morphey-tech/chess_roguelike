using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    /// <summary>
    /// Executes move action.
    /// 
    /// PIPELINE:
    /// 1. Domain: MovementService updates grid state
    /// 2. Visual: VisualPipeline plays move animation
    /// </summary>
    public sealed class MoveStep : ITurnStep
    {
        public string Id { get; }

        private readonly MovementService _movementService;
        private readonly VisualPipeline _visualPipeline;

        public MoveStep(string id, MovementService movementService, VisualPipeline visualPipeline)
        {
            Id = id;
            _movementService = movementService;
            _visualPipeline = visualPipeline;
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            if (!_movementService.CanMove(context.From, context.To))
                return;

            // === DOMAIN ===
            _movementService.MoveFigure(context.From, context.To);
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
