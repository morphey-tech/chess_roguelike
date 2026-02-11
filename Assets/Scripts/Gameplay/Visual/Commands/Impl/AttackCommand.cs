using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play attack animation.
    /// </summary>
    public sealed class AttackCommand : IVisualCommand
    {
        private readonly AttackVisualContext _ctx;

        public string DebugName => $"Attack(attacker={_ctx.AttackerId}, target=[{_ctx.TargetPosition.Row},{_ctx.TargetPosition.Column}]{(_ctx.AttackType != null ? $", type={_ctx.AttackType}" : "")})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public AttackCommand(AttackVisualContext ctx)
        {
            _ctx = ctx;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Figures.PlayAttackAsync(_ctx.AttackerId, _ctx.TargetPosition);
        }
    }
}
