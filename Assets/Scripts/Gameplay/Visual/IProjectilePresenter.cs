using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Executes projectile/beam/wave visuals. Только показ, без домена.
    /// Fly → Impact → Death/Loot → Cleanup идут отдельными командами в очереди.
    /// </summary>
    public interface IProjectilePresenter
    {
        UniTask FlyProjectileAsync(ProjectileVisualContext ctx);
        UniTask PlayImpactAtAsync(GridPosition position, string impactFxId = null);
        UniTask CleanupLastProjectileAsync();
        UniTask PlayBeamAsync(BeamVisualContext ctx);
        UniTask PlayWaveAsync(WaveVisualContext ctx);
        void Clear();
    }
}
