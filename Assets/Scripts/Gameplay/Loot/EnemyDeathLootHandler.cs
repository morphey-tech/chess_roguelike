using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Loot
{
    /// <summary>
    /// Subscribes to figure death; when an enemy dies, rolls its loot table.
    /// Enemy does not know what it drops — only reports "I died, my loot table is X".
    /// </summary>
    public sealed class EnemyDeathLootHandler : IStartable, IDisposable
    {
        private readonly LootService _lootService;
        private readonly ISubscriber<FigureDeathMessage> _deathSubscriber;
        private readonly ILogger _logger;
        private IDisposable? _subscription;

        public EnemyDeathLootHandler(
            LootService lootService,
            ISubscriber<FigureDeathMessage> deathSubscriber,
            ILogService logService)
        {
            _lootService = lootService;
            _deathSubscriber = deathSubscriber;
            _logger = logService.CreateLogger<EnemyDeathLootHandler>();
        }

        public void Start()
        {
            _subscription = _deathSubscriber.Subscribe(OnFigureDeath);
            _logger.Debug("EnemyDeathLootHandler subscribed to FigureDeathMessage");
        }

        private void OnFigureDeath(FigureDeathMessage message)
        {
            if (message.Team != Team.Enemy)
                return;
            if (message.FromCombat)
                return; // Loot already handled by LootVisualEvent → LootPresenter
            if (string.IsNullOrEmpty(message.LootTableId))
                return;

            _lootService.RollAsync(message.LootTableId).Forget();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
