using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.UI;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to show damage text on a figure.
    /// </summary>
    public sealed class DamageTextCommand : IVisualCommand
    {
        private readonly DamageVisualContext _ctx;
        private readonly VisualCommandMode _mode;

        public string DebugName => $"DamageText(target={_ctx.TargetId}, amount={_ctx.Amount}{(_ctx.IsCritical ? ", CRIT" : "")}{(_ctx.DamageType != null ? $", type={_ctx.DamageType}" : "")})";
        public VisualCommandMode Mode => _mode;

        public DamageTextCommand(DamageVisualContext ctx, VisualCommandMode mode = VisualCommandMode.Blocking)
        {
            _ctx = ctx;
            _mode = mode;
        }

        public async UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            await presenters.Figures.ShowDamageText(_ctx.TargetId,  _ctx);
        }
    }
}
