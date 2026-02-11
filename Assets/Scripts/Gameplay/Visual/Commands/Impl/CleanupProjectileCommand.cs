using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Removes the last flown projectile from the scene (после Death/Loot в очереди).
    /// </summary>
    public sealed class CleanupProjectileCommand : IVisualCommand
    {
        public string DebugName => "CleanupProjectile";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Projectiles.CleanupLastProjectileAsync();
        }
    }
}
