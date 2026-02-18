using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnService : IDisposable
    {
        public Team CurrentTeam { get; private set; } = Team.Player;
        public int TurnNumber { get; private set; } = 1;

        private readonly IPublisher<TurnChangedMessage> _turnChangedPublisher;
        private readonly IGameUiService _uiService;
        private readonly PassiveTriggerService _passiveTriggerService;
        private readonly RunHolder _runHolder;
        private readonly IFigureRegistry _figureRegistry;
        private readonly ILogger<TurnService> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private TurnService(
            ISubscriber<EndTurnRequestedMessage> endTurnSubscriber,
            IPublisher<TurnChangedMessage> turnChangedPublisher,
            IGameUiService uiService,
            PassiveTriggerService passiveTriggerService,
            RunHolder runHolder,
            IFigureRegistry figureRegistry,
            ILogService logService)
        {
            _turnChangedPublisher = turnChangedPublisher;
            _uiService = uiService;
            _passiveTriggerService = passiveTriggerService;
            _runHolder = runHolder;
            _figureRegistry = figureRegistry;
            _logger = logService.CreateLogger<TurnService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            endTurnSubscriber.Subscribe(_ => OnEndTurnRequested()).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnEndTurnRequested()
        {
            EndTurn();
        }

        public void EndTurn()
        {
            BoardGrid? boardGrid = _runHolder.Current.CurrentStage!.Grid;
            TurnContext turnContext = new()
            {
                Grid = boardGrid ?? throw new NullReferenceException("Board Grid not set"),
                Team = CurrentTeam,
                CurrentTurn = TurnNumber
            };

            _passiveTriggerService.TriggerTurnEnd(turnContext);

            foreach (Figure figure in _figureRegistry.GetAll())
                figure.Stats.TickTimedModifiers();

            Team previousTeam = CurrentTeam;
            CurrentTeam = CurrentTeam == Team.Player ? Team.Enemy : Team.Player;
            TurnNumber++;

            foreach (Figure figure in _figureRegistry.GetAll())
                figure.MovedThisTurn = false;

            _passiveTriggerService.TriggerTurnStart(new TurnContext
            {
                Grid = boardGrid ?? throw new NullReferenceException("Board Grid not set"),
                Team = CurrentTeam,
                CurrentTurn = TurnNumber
            });
            _turnChangedPublisher.Publish(new TurnChangedMessage(CurrentTeam, TurnNumber));
            _logger.Info($"Turn ended: {previousTeam} -> {CurrentTeam}, Turn #{TurnNumber}");
        }

        public void StartBattle()
        {
            TurnNumber = 1;
            CurrentTeam = Team.Player;
            _uiService.SetGamePhase();
            _turnChangedPublisher.Publish(new TurnChangedMessage(CurrentTeam, TurnNumber));
        }

        void IDisposable.Dispose()
        {  
            _subscriptions?.Dispose();
        }
    }
}
