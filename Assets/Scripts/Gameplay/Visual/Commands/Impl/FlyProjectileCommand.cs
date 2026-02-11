using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Only flies projectile from A to B. No domain, no hit. Cleanup — отдельной командой.
    /// </summary>
    public sealed class FlyProjectileCommand : IVisualCommand
    {
        private readonly ProjectileVisualContext _ctx;

        public string DebugName => $"FlyProjectile(attacker={_ctx.AttackerId}, to=[{_ctx.To.Row},{_ctx.To.Column}])";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public FlyProjectileCommand(ProjectileVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Projectiles.FlyProjectileAsync(_ctx);
        }
    }
}
