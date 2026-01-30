using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class MoveStep : ITurnStep
    {
        public string Id { get; }

        private readonly MovementService _movementService;
        private readonly IFigurePresenter _figurePresenter;

        public MoveStep(string id, MovementService movementService, IFigurePresenter figurePresenter)
        {
            Id = id;
            _movementService = movementService;
            _figurePresenter = figurePresenter;
        }

        public UniTask ExecuteAsync(ActionContext context)
        {
            if (!_movementService.CanMove(context.From, context.To))
                return UniTask.CompletedTask;

            _movementService.MoveFigure(context.From, context.To);
            _figurePresenter.MoveFigure(context.Actor.Id, context.To);

            context.From = context.To;
            return UniTask.CompletedTask;
        }
    }
}
