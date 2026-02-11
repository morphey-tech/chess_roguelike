using System.Collections.Generic;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Visual.Commands;
using VContainer;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Default implementation of IPresenterProvider. Реализует IVisualQueueAppender для добавления команд во время исполнения.
    /// </summary>
    public sealed class PresenterProvider : IPresenterProvider, IVisualQueueAppender
    {
        public IBoardPresenter Board { get; }
        public IFigurePresenter Figures { get; }
        public IPreparePresenter Prepare { get; }
        public IProjectilePresenter Projectiles { get; }
        public IProjectileHitApplyService ProjectileHitApplier { get; }
        public ILootPresenter? Loot { get; }
        public IVisualQueueAppender? QueueAppender => this;

        private VisualCommandQueue _currentQueue;

        [Inject]
        private PresenterProvider(
            IFigurePresenter figurePresenter,
            IBoardPresenter board,
            IPreparePresenter prepare,
            IProjectilePresenter projectiles,
            IProjectileHitApplyService projectileHitApplier,
            ILootPresenter? loot = null)
        {
            Figures = figurePresenter;
            Board = board;
            Prepare = prepare;
            Projectiles = projectiles;
            ProjectileHitApplier = projectileHitApplier;
            Loot = loot;
        }

        public void SetCurrentQueue(VisualCommandQueue queue)
        {
            _currentQueue = queue;
        }

        public void EnqueueCommands(IReadOnlyList<IVisualCommand> commands)
        {
            _currentQueue?.EnqueueRange(commands);
        }
    }
}
