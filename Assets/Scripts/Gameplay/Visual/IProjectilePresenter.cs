using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Executes projectile/beam/wave visuals.
    /// </summary>
    public interface IProjectilePresenter
    {
        UniTask PlayProjectileAsync(ProjectileVisualContext ctx);
        UniTask PlayBeamAsync(BeamVisualContext ctx);
        UniTask PlayWaveAsync(WaveVisualContext ctx);
        void Clear();
    }

    /// <summary>
    /// Domain callback invoked when projectile hits.
    /// </summary>
    public interface IProjectileHitHandler
    {
        void OnProjectileHit(ProjectileVisualContext ctx);
    }
}
