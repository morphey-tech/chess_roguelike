using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
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
        private readonly AttackStrategyFactory _attackFactory;
        private readonly CombatResolver _combatResolver;
        private readonly TurnSystem _turnSystem;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ILogger<StageService> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private StageService(
            RunHolder runHolder,
            MovementService movementService,
            AttackStrategyFactory attackFactory,
            CombatResolver combatResolver,
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
            _attackFactory = attackFactory;
            _combatResolver = combatResolver;
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
            BoardGrid grid = currentStage?.Grid;
            BoardCell fromCell = grid?.GetBoardCell(message.From);
            BoardCell toCell = grid?.GetBoardCell(message.To);
            Figure attacker = fromCell?.OccupiedBy;

            if (attacker == null)
            {
                _logger.Error("No figure at source position!");
                return;
            }

            Figure defender = toCell?.OccupiedBy;
            if (defender != null)
            {
                // 1. Get attack strategy and create hit context
                IAttackStrategy attackStrategy = _attackFactory.Get(attacker.AttackId);
                HitContext hitContext = attackStrategy.CreateHitContext(attacker, defender, message.From, message.To, grid);
                
                // 2. Resolve combat through effect pipeline
                CombatResult result = _combatResolver.Resolve(hitContext);
                
                _logger.Info($"{attacker} [{attackStrategy.Id}] attacks {defender} for {result.DamageDealt} damage. HP: {defender.Stats.CurrentHp}/{defender.Stats.MaxHp}");
                
                if (result.HealedAmount > 0)
                {
                    _logger.Info($"{attacker} healed for {result.HealedAmount}. HP: {attacker.Stats.CurrentHp}/{attacker.Stats.MaxHp}");
                }
                
                // Play attack animation
                _figurePresenter.PlayAttack(attacker.Id, message.To);
                _figurePresenter.PlayDamageEffect(defender.Id);

                if (result.TargetDied)
                {
                    _logger.Info($"{defender} died!");
                    toCell.RemoveFigure();
                    _figurePresenter.RemoveFigure(defender.Id);
                    _deathPublisher.Publish(new FigureDeathMessage(defender.Id, defender.Team));
                }

                // Move based on combat result
                if (result.AttackerMoves)
                {
                    _movementService.MoveFigure(message.From, message.To);
                    _figurePresenter.MoveFigure(attacker.Id, message.To);
                }
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
