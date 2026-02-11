using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Plays impact VFX at position (e.g. projectile hit).
    /// </summary>
    public sealed class ImpactCommand : IVisualCommand
    {
        private readonly ImpactVisualContext _ctx;

        public string DebugName => $"Impact(pos=[{_ctx.Position.Row},{_ctx.Position.Column}])";
        public VisualCommandMode Mode => VisualCommandMode.Background;

        public ImpactCommand(ImpactVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Projectiles.PlayImpactAtAsync(_ctx.Position, _ctx.ImpactFxId);
        }
    }
}
