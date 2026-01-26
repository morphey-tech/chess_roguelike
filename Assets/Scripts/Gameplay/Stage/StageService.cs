using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.Conditions;
using Project.Gameplay.Gameplay.Turn.Steps;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Stage
{
    public class StageService : IStartable, IDisposable
    {
        private readonly RunHolder _runHolder;
        private readonly TurnPatternResolver _patternResolver;
        private readonly TurnSystem _turnSystem;
        private readonly ILogger<StageService> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private StageService(
            RunHolder runHolder,
            TurnPatternResolver patternResolver,
            TurnSystem turnSystem,
            ISubscriber<FigureSpawnedMessage> figureSpawnedSubscriber,
            ISubscriber<MoveRequestedMessage> moveSubscriber,
            ISubscriber<FigureSelectedMessage> selectionSubscriber,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ILogService logService)
        {
            _runHolder = runHolder;
            _patternResolver = patternResolver;
            _turnSystem = turnSystem;
            _logger = logService.CreateLogger<StageService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            figureSpawnedSubscriber.Subscribe(OnFigureSpawned).AddTo(bag);
            moveSubscriber.Subscribe(OnMoveRequested).AddTo(bag);
            selectionSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
            turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            _subscriptions = bag.Build();

            _logger.Info("StageService created");
        }

        void IStartable.Start()
        {
            _logger.Info("StageService started");
        }

        private void OnFigureSpawned(FigureSpawnedMessage message)
        {
            _logger.Info($"Figure {message.Figure.Id} spawned at ({message.Position.Row}, {message.Position.Column})");
        }

        private void OnMoveRequested(MoveRequestedMessage message)
        {
            _logger.Info($"Move requested: ({message.From.Row},{message.From.Column}) -> ({message.To.Row},{message.To.Column})");

            Stage currentStage = _runHolder.Current?.CurrentStage;
            BoardGrid grid = currentStage?.Grid;
            BoardCell fromCell = grid?.GetBoardCell(message.From);
            Figure actor = fromCell?.OccupiedBy;

            if (actor == null)
            {
                _logger.Error("No figure at source position!");
                return;
            }

            if (actor.TurnPatternSet == null)
            {
                _logger.Error($"Figure {actor} has no TurnPatternSet!");
                return;
            }

            List<Figure> enemies = grid.GetFiguresByTeam(actor.Team == Team.Player ? Team.Enemy : Team.Player)
                .ToList();

            var selectionContext = new TurnSelectionContext
            {
                Actor = actor,
                Grid = grid,
                ActorPosition = message.From,
                TargetPosition = message.To,
                Enemies = enemies
            };

            ITurnStep step = _patternResolver.Resolve(actor, actor.TurnPatternSet, selectionContext);

            if (step == null)
            {
                _logger.Debug($"No valid pattern for {actor}");
                return;
            }

            var stepContext = new TurnStepContext
            {
                Actor = actor,
                Grid = grid,
                From = message.From,
                To = message.To
            };

            ExecuteTurnAsync(actor, step, stepContext).Forget();
        }

        private async UniTaskVoid ExecuteTurnAsync(Figure actor, ITurnStep step, TurnStepContext context)
        {
            _logger.Info($"{actor} executing pattern step: {step.Id}");
            
            await step.ExecuteAsync(context);
            
            _logger.Info($"Turn completed for {actor}");
            _turnSystem.EndTurn();
        }

        private void OnFigureSelected(FigureSelectedMessage message)
        {
            if (message.Figure != null)
            {
                _logger.Info($"Figure {message.Figure.Id} selected at ({message.Position.Row}, {message.Position.Column})");
            }
            else
            {
                _logger.Debug("Selection cleared");
            }
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            _logger.Info($"Turn {message.TurnNumber}: {message.CurrentTeam}'s turn");
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
