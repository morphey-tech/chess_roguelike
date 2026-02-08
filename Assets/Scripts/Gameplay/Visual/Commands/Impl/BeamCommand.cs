using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command for beam delivery (placeholder).
    /// </summary>
    public sealed class BeamCommand : IVisualCommand
    {
        private readonly BeamVisualContext _ctx;

        public string DebugName => $"Beam(attacker={_ctx.AttackerId}, target={_ctx.TargetId}, from=[{_ctx.From.Row},{_ctx.From.Column}], to=[{_ctx.To.Row},{_ctx.To.Column}]{(_ctx.AttackType != null ? $", type={_ctx.AttackType}" : "")})";

        public BeamCommand(BeamVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Projectiles.PlayBeamAsync(_ctx);
        }
    }
}
