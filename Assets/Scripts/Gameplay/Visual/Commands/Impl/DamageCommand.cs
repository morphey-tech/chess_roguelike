using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play damage effect on a figure.
    /// </summary>
    public sealed class DamageCommand : IVisualCommand
    {
        private readonly DamageVisualContext _ctx;
        private readonly VisualCommandMode _mode;

        public string DebugName => $"Damage(target={_ctx.TargetId}, amount={_ctx.Amount}{(_ctx.IsCritical ? ", CRIT" : "")}{(_ctx.DamageType != null ? $", type={_ctx.DamageType}" : "")})";
        public VisualCommandMode Mode => _mode;

        public DamageCommand(DamageVisualContext ctx, VisualCommandMode mode = VisualCommandMode.Blocking)
        {
            _ctx = ctx;
            _mode = mode;
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
