using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Prepare;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Provides access to presenters and services for visual commands.
    /// </summary>
    public interface IPresenterProvider
    {
        IBoardPresenter Board { get; }
        IFigurePresenter Figures { get; }
        IPreparePresenter Prepare { get; }
        IProjectilePresenter Projectiles { get; }
        IProjectileHitApplyService ProjectileHitApplier { get; }
        /// <summary>Can be null; LootCommand no-ops if null.</summary>
        ILootPresenter? Loot { get; }
        /// <summary>During execution, allows commands to append (e.g. Death+Loot after projectile hit).</summary>
        IVisualQueueAppender? QueueAppender { get; }
    }
}
