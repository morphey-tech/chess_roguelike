using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Gameplay.Turn.Execution;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Stage coordinator - listens to events and delegates to appropriate services.
    /// Does NOT contain game logic, only orchestration.
    /// </summary>
    public class StageService : IStartable, IDisposable
    {
        private readonly RunHolder _runHolder;
        private readonly ITurnExecutor _turnExecutor;
        private readonly IBonusMoveController _bonusMoveController;
        private readonly IBoardPresenter _boardPresenter;
        private readonly MovementService _movementService;
        private readonly TurnSystem _turnSystem;
        private readonly ILogger<StageService> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private StageService(
            RunHolder runHolder,
            ITurnExecutor turnExecutor,
            IBonusMoveController bonusMoveController,
            IBoardPresenter boardPresenter,
            MovementService movementService,
            TurnSystem turnSystem,
            ISubscriber<FigureSpawnedMessage> figureSpawnedSubscriber,
            ISubscriber<MoveRequestedMessage> moveSubscriber,
            ISubscriber<AttackRequestedMessage> attackSubscriber,
            ISubscriber<FigureSelectedMessage> selectionSubscriber,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ISubscriber<BonusMoveCompletedMessage> bonusMoveCompletedSubscriber,
            ILogService logService)
        {
            _runHolder = runHolder;
            _turnExecutor = turnExecutor;
            _bonusMoveController = bonusMoveController;
            _boardPresenter = boardPresenter;
            _movementService = movementService;
            _turnSystem = turnSystem;
            _logger = logService.CreateLogger<StageService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            figureSpawnedSubscriber.Subscribe(OnFigureSpawned).AddTo(bag);
            moveSubscriber.Subscribe(OnMoveRequested).AddTo(bag);
            attackSubscriber.Subscribe(OnAttackRequested).AddTo(bag);
            selectionSubscriber.Subscribe(OnFigureSelected).AddTo(bag);
            turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            bonusMoveCompletedSubscriber.Subscribe(OnBonusMoveCompleted).AddTo(bag);
            _subscriptions = bag.Build();

            _logger.Info("StageService created");
        }

        void IStartable.Start()
        {
            _logger.Info("StageService started");
        }

        private void OnFigureSpawned(FigureSpawnedMessage message)
        {
            _logger.Debug($"Figure {message.Figure.Id} spawned at ({message.Position.Row}, {message.Position.Column})");
        }

        private void OnMoveRequested(MoveRequestedMessage message)
        {
            if (_bonusMoveController.IsActive)
                return;

            _logger.Info($"Move: ({message.From.Row},{message.From.Column}) -> ({message.To.Row},{message.To.Column})");
            TryExecuteTurn(message.From, message.To);
        }

        private void OnAttackRequested(AttackRequestedMessage message)
        {
            if (_bonusMoveController.IsActive)
                return;

            _logger.Info($"Attack: ({message.From.Row},{message.From.Column}) -> ({message.To.Row},{message.To.Column})");
            TryExecuteTurn(message.From, message.To);
        }

        private void TryExecuteTurn(GridPosition from, GridPosition to)
        {
            BoardGrid grid = GetCurrentGrid();
            if (grid == null) return;

            BoardCell fromCell = grid.GetBoardCell(from);
            Figure actor = fromCell?.OccupiedBy;

            if (actor == null)
            {
                _logger.Error("No figure at source position!");
                return;
            }

            ExecuteTurnAsync(actor, from, to, grid).Forget();
        }

        private void OnBonusMoveCompleted(BonusMoveCompletedMessage message)
        {
            _logger.Info($"Bonus move completed for {message.Actor}");
            ClearHighlights();
            _turnSystem.EndTurn();
        }

        private async UniTaskVoid ExecuteTurnAsync(Figure actor, GridPosition from, GridPosition to, BoardGrid grid)
        {
            TurnExecutionResult result = await _turnExecutor.ExecuteAsync(actor, from, to, grid);

            if (!result.Success)
            {
                _logger.Debug($"Turn execution failed for {actor}");
                return;
            }

            // Check if bonus move was granted
            if (result.BonusMoveDistance.HasValue && result.BonusMoveDistance.Value > 0)
            {
                _logger.Info($"{actor} gets bonus move! Max distance: {result.BonusMoveDistance.Value}");
                _bonusMoveController.Start(actor, result.ActorFinalPosition, result.BonusMoveDistance.Value, grid);
                HighlightPositions(_bonusMoveController.GetAvailablePositions());
                return;
            }

            _logger.Info($"Turn completed for {actor}");
            _turnSystem.EndTurn();
        }

        private void OnFigureSelected(FigureSelectedMessage message)
        {
            // Don't change highlights during bonus move
            if (_bonusMoveController.IsActive)
                return;

            if (message.Figure != null)
            {
                _logger.Debug($"Figure {message.Figure.Id} selected at ({message.Position.Row}, {message.Position.Column})");
                HighlightPositions(_movementService.GetAvailableMoves(message.Figure, message.Position));
            }
            else
            {
                ClearHighlights();
                _logger.Debug("Selection cleared");
            }
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            _logger.Info($"Turn {message.TurnNumber}: {message.CurrentTeam}'s turn");
            
            // Cancel any pending bonus move on turn change
            if (_bonusMoveController.IsActive)
            {
                _bonusMoveController.Cancel();
                ClearHighlights();
            }
        }

        private BoardGrid GetCurrentGrid()
        {
            return _runHolder.Current?.CurrentStage?.Grid;
        }

        private void HighlightPositions(IEnumerable<GridPosition> positions)
        {
            BoardGrid grid = GetCurrentGrid();
            if (grid == null) return;

            HashSet<GridPosition> highlightSet = positions?.ToHashSet() ?? new HashSet<GridPosition>();
            
            foreach (BoardCell cell in grid.AllCells())
            {
                _boardPresenter.Highlight(cell.Position, highlightSet.Contains(cell.Position));
            }
        }

        private void ClearHighlights()
        {
            HighlightPositions(null);
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
