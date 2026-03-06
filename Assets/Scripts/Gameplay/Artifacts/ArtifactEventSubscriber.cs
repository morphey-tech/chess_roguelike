using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.ShrinkingZone.Messages;
using VContainer;

namespace Project.Gameplay.Gameplay.Artifacts
{
    public sealed class ArtifactEventSubscriber : IDisposable
    {
        private readonly ArtifactTriggerService _triggerService;
        private readonly IDisposable? _subscription;
        private readonly ILogger<ArtifactEventSubscriber> _logger;

        [Inject]
        private ArtifactEventSubscriber(
            ArtifactTriggerService triggerService,
            ISubscriber<StormBattleStartedMessage> battleStartedSubscriber,
            ISubscriber<FigureDeathMessage> deathSubscriber,
            ILogService logService)
        {
            _triggerService = triggerService;
            _logger = logService.CreateLogger<ArtifactEventSubscriber>();
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            battleStartedSubscriber.Subscribe(_ =>
            {
                _triggerService.TriggerBattleStart();
            }).AddTo(bag);
            deathSubscriber.Subscribe(OnFigureDeath).AddTo(bag);
            _subscription = bag.Build();
        }

        private void OnFigureDeath(FigureDeathMessage message)
        {
            // Trigger OnUnitDeath for all artifacts
            _triggerService.TriggerUnitDeath(message.FigureId);

            // Note: OnUnitKill would need to know who killed
            // This requires extending FigureDeathMessage with KillerId
            
            // OnAllyDeath would need team information
            // For now, just trigger generic death
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
