using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command for projectile delivery (placeholder).
    /// </summary>
    public sealed class ProjectileCommand : IVisualCommand
    {
        private readonly ProjectileVisualContext _ctx;

        public string DebugName => $"Projectile(attacker={_ctx.AttackerId}, target={_ctx.TargetEntityId}, dmg={_ctx.Damage}{(_ctx.IsCritical ? ", CRIT" : "")}, from=[{_ctx.From.Row},{_ctx.From.Column}], to=[{_ctx.To.Row},{_ctx.To.Column}]{(_ctx.AttackType != null ? $", type={_ctx.AttackType}" : "")})";

        public ProjectileCommand(ProjectileVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Projectiles.PlayProjectileAsync(_ctx);
        }
    }
}
