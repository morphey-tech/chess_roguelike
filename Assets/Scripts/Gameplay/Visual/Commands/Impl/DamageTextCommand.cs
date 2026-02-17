using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.UI;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play damage effect on a figure.
    /// </summary>
    public sealed class DamageTextCommand : IVisualCommand
    {
        private readonly DamageVisualContext _ctx;

        public string DebugName => $"DamageText(target={_ctx.TargetId}, amount={_ctx.Amount}{(_ctx.IsCritical ? ", CRIT" : "")}{(_ctx.DamageType != null ? $", type={_ctx.DamageType}" : "")})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public DamageTextCommand(DamageVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        { 
            presenters.Figures.ShowDamageText(_ctx.TargetId,  _ctx);
            return UniTask.CompletedTask;
        }
    }
}
