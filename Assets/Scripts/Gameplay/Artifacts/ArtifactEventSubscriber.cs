using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.ShrinkingZone.Messages;
using VContainer;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Subscribes to game events and triggers artifact effects.
    /// </summary>
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
            // Note: FigureDeathMessage needs to be extended with KillerId for full functionality
            _triggerService.TriggerUnitDeath(
                victim: GetFigureById(message.FigureId),
                killer: null // TODO: Get killer from message
            );
        }

        private static Figure? GetFigureById(int figureId)
        {
            // TODO: Resolve from FigureRegistry or EntityService
            return null;
        }

        void IDisposable.Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
