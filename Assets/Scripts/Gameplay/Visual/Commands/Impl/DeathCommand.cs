using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play death effect and remove figure.
    /// </summary>
    public sealed class DeathCommand : IVisualCommand
    {
        private readonly DeathVisualContext _ctx;

        public string DebugName => $"Death(figure={_ctx.FigureId}{(_ctx.DeathReason != null ? $", reason={_ctx.DeathReason}" : "")})";

        public DeathCommand(DeathVisualContext ctx)
        {
            _ctx = ctx;
        }

        public async UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            await presenters.Figures.PlayDeathEffectAsync(_ctx.FigureId);
            await presenters.Figures.RemoveFigureAsync(_ctx.FigureId);
        }
    }
}
