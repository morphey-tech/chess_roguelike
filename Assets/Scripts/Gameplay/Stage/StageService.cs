using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Turn;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Handles stage events: figure movement, selection, turn changes.
    /// Stage flow (board spawn, figure spawn) is handled by Stage itself.
    /// No Unity dependencies - visualization through IFigureView.
    /// </summary>
    public class StageService : IStartable, IDisposable
    {
        private readonly RunHolder _runHolder;
        private readonly MovementService _movementService;
        private readonly TurnSystem _turnSystem;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ILogger<StageService> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private StageService(
            RunHolder runHolder,
            MovementService movementService,
            TurnSystem turnSystem,
            IFigurePresenter figurePresenter,
            IPublisher<FigureDeathMessage> deathPublisher,
            ISubscriber<FigureSpawnedMessage> figureSpawnedSubscriber,
            ISubscriber<MoveRequestedMessage> moveSubscriber,
            ISubscriber<FigureSelectedMessage> selectionSubscriber,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ILogService logService)
        {
            _runHolder = runHolder;
            _movementService = movementService;
            _turnSystem = turnSystem;
            _figurePresenter = figurePresenter;
            _deathPublisher = deathPublisher;
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

            if (!_movementService.CanMove(message.From, message.To))
            {
                _logger.Debug("Move rejected by MovementService");
                return;
            }

            Stage currentStage = _runHolder.Current?.CurrentStage;
            BoardCell fromCell = currentStage?.Grid.GetBoardCell(message.From);
            BoardCell toCell = currentStage?.Grid.GetBoardCell(message.To);
            Figure attacker = fromCell?.OccupiedBy;

            if (attacker == null)
            {
                _logger.Error("No figure at source position!");
                return;
            }

            Figure defender = toCell?.OccupiedBy;
            if (defender != null)
            {
                // Combat!
                int damage = attacker.Stats.Attack;
                bool defenderDied = defender.Stats.TakeDamage(damage);
                
                _logger.Info($"{attacker} attacks {defender} for {damage} damage. HP: {defender.Stats.CurrentHp}/{defender.Stats.MaxHp}");
                
                // Play attack animation
                _figurePresenter.PlayAttack(attacker.Id, message.To);
                _figurePresenter.PlayDamageEffect(defender.Id);

                if (defenderDied)
                {
                    _logger.Info($"{defender} died!");
                    toCell.RemoveFigure();
                    _figurePresenter.RemoveFigure(defender.Id);
                    _deathPublisher.Publish(new FigureDeathMessage(defender.Id, defender.Team));
                    
                    // Move to the empty cell
                    _movementService.MoveFigure(message.From, message.To);
                    _figurePresenter.MoveFigure(attacker.Id, message.To);
                }
                // If defender survives - attacker doesn't move
            }
            else
            {
                // Simple move to empty cell
                _movementService.MoveFigure(message.From, message.To);
                _figurePresenter.MoveFigure(attacker.Id, message.To);
            }

            _logger.Info($"Turn completed for {attacker}");
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
