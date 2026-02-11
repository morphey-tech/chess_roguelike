using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play push effect (knockback animation).
    /// </summary>
    public sealed class PushCommand : IVisualCommand
    {
        private readonly PushVisualContext _ctx;

        public string DebugName => $"Push(figure={_ctx.FigureId}, from=[{_ctx.From.Row},{_ctx.From.Column}], to=[{_ctx.To.Row},{_ctx.To.Column}])";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public PushCommand(PushVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Figures.PlayPushEffectAsync(_ctx.FigureId, _ctx.From, _ctx.To);
        }
    }
}
