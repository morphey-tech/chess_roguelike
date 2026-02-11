using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command for wave delivery (placeholder).
    /// </summary>
    public sealed class WaveCommand : IVisualCommand
    {
        private readonly WaveVisualContext _ctx;

        public string DebugName => $"Wave(attacker={_ctx.AttackerId}, target={_ctx.TargetId}, from=[{_ctx.From.Row},{_ctx.From.Column}], to=[{_ctx.To.Row},{_ctx.To.Column}]{(_ctx.AttackType != null ? $", type={_ctx.AttackType}" : "")})";
        public VisualCommandMode Mode => VisualCommandMode.Background;

        public WaveCommand(WaveVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Projectiles.PlayWaveAsync(_ctx);
        }
    }
}
