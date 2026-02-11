using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Применяет урон снаряда после Impact+Cleanup (домен + смерть/лут через HandleDeathDirectAsync).
    /// </summary>
    public sealed class ProjectileHitApplyCommand : IVisualCommand
    {
        private readonly ProjectileHitApplyEvent _evt;

        public string DebugName => $"ProjectileHitApply(target={_evt.TargetId}, dmg={_evt.Damage})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public ProjectileHitApplyCommand(ProjectileHitApplyEvent evt)
        {
            _evt = evt;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.ProjectileHitApplier.ApplyAsync(_evt, presenters);
        }
    }
}
