using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;

namespace Project.Gameplay.Gameplay.Turn
{
    public class TurnSystem : IDisposable
    {
        private readonly IPublisher<TurnChangedMessage> _turnChangedPublisher;
        private readonly ILogger<TurnSystem> _logger;
        private readonly IDisposable _subscriptions;

        public Team CurrentTeam { get; private set; } = Team.Player;
        public int TurnNumber { get; private set; } = 1;

        public TurnSystem(
            ISubscriber<EndTurnRequestedMessage> endTurnSubscriber,
            IPublisher<TurnChangedMessage> turnChangedPublisher,
            ILogService logService)
        {
            _turnChangedPublisher = turnChangedPublisher;
            _logger = logService.CreateLogger<TurnSystem>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            endTurnSubscriber.Subscribe(_ => OnEndTurnRequested()).AddTo(bag);
            _subscriptions = bag.Build();
            
            _logger.Info("TurnSystem created with input subscriptions");
        }

        private void OnEndTurnRequested()
        {
            EndTurn();
        }

        public void EndTurn()
        {
            Team previousTeam = CurrentTeam;
            CurrentTeam = CurrentTeam == Team.Player ? Team.Enemy : Team.Player;
            
            if (CurrentTeam == Team.Player)
            {
                TurnNumber++;
            }

            _logger.Info($"Turn ended: {previousTeam} -> {CurrentTeam}, Turn #{TurnNumber}");
            _turnChangedPublisher.Publish(new TurnChangedMessage(CurrentTeam, TurnNumber));
        }

        public bool IsPlayerTurn => CurrentTeam == Team.Player;

        public void Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
