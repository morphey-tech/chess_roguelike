using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play damage effect on a figure.
    /// </summary>
    public sealed class DamageCommand : IVisualCommand
    {
        private readonly DamageVisualContext _ctx;

        public string DebugName => $"Damage(target={_ctx.TargetId}, amount={_ctx.Amount}{(_ctx.IsCritical ? ", CRIT" : "")}{(_ctx.DamageType != null ? $", type={_ctx.DamageType}" : "")})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public DamageCommand(DamageVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            if (_ctx.IsDodged)
            {
                return  UniTask.CompletedTask;
            }
            return presenters.Figures.PlayDamageEffectAsync(_ctx.TargetId);
        }
    }
}
