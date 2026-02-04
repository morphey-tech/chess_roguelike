using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to move a figure to a new position.
    /// </summary>
    public sealed class MoveCommand : IVisualCommand
    {
        private readonly MoveVisualContext _ctx;

        public string DebugName => $"Move(figure={_ctx.FigureId}, to=[{_ctx.To.Row},{_ctx.To.Column}])";

        public MoveCommand(MoveVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Figures.MoveFigureAsync(_ctx.FigureId, _ctx.To);
        }
    }
}
