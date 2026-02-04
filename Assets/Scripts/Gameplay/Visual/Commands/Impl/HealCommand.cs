using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play heal effect on a figure.
    /// </summary>
    public sealed class HealCommand : IVisualCommand
    {
        private readonly HealVisualContext _ctx;

        public string DebugName => $"Heal(target={_ctx.TargetId}, amount={_ctx.Amount})";

        public HealCommand(HealVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Figures.PlayHealEffectAsync(_ctx.TargetId);
        }
    }
}
