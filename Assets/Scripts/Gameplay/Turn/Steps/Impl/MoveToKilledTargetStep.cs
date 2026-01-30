using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class MoveToKilledTargetStep : ITurnStep
    {
        public string Id => "move_to_killed";

        private readonly MovementService _movementService;
        private readonly IFigurePresenter _figurePresenter;

        public MoveToKilledTargetStep(MovementService movementService, IFigurePresenter figurePresenter)
        {
            _movementService = movementService;
            _figurePresenter = figurePresenter;
        }

        public UniTask ExecuteAsync(ActionContext context)
        {
            if (!context.LastAttackKilledTarget)
                return UniTask.CompletedTask;

            int distance = Attack.AttackUtils.GetDistance(context.From, context.To);
            if (distance != 1)
                return UniTask.CompletedTask;

            _movementService.MoveFigure(context.From, context.To);
            _figurePresenter.MoveFigure(context.Actor.Id, context.To);
            context.From = context.To;

            return UniTask.CompletedTask;
        }
    }
}
