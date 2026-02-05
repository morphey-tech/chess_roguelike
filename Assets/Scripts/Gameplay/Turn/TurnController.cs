using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Gameplay.Turn.Execution;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;
using VContainer;

namespace Project.Gameplay.Gameplay.Turn
{
    /// <summary>
    /// Controller for executing player turns.
    /// Acquires interaction lock, executes turns via TurnExecutor, handles bonus moves, and ends turns.
    /// 
    /// During bonus move:
    /// - Holds the interaction lock (blocking normal interaction)
    /// - Receives cell clicks directly and forwards to BonusMoveController (domain)
    /// - Plays move animation via VisualPipeline (visual)
    /// - Publishes highlight messages for StageService
    /// </summary>
    public sealed class TurnController : ITurnController, IDisposable
    {
        private readonly IInteractionLock _interactionLock;
        private readonly ITurnExecutor _turnExecutor;
        private readonly IBonusMoveController _bonusMoveController;
        private readonly VisualPipeline _visualPipeline;
        private readonly TurnSystem _turnSystem;
        private readonly RunHolder _runHolder;
        private readonly IPublisher<BonusMoveStartedMessage> _bonusMoveStartedPublisher;
        private readonly IPublisher<BonusMoveCompletedMessage> _bonusMoveCompletedPublisher;
        private readonly ILogger<TurnController> _logger;
        private readonly IDisposable _subscriptions;

        // Bonus move click handling
        private UniTaskCompletionSource<GridPosition>? _pendingClickTcs;
        private bool _isInBonusMoveMode;
        private Figure _bonusMoveActor; // Store actor for visual command

        [Inject]
        public TurnController(
            IInteractionLock interactionLock,
            ITurnExecutor turnExecutor,
            IBonusMoveController bonusMoveController,
            VisualPipeline visualPipeline,
            TurnSystem turnSystem,
            RunHolder runHolder,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            IPublisher<BonusMoveStartedMessage> bonusMoveStartedPublisher,
            IPublisher<BonusMoveCompletedMessage> bonusMoveCompletedPublisher,
            ILogService logService)
        {
            _interactionLock = interactionLock;
            _turnExecutor = turnExecutor;
            _bonusMoveController = bonusMoveController;
            _visualPipeline = visualPipeline;
            _turnSystem = turnSystem;
            _runHolder = runHolder;
            _bonusMoveStartedPublisher = bonusMoveStartedPublisher;
            _bonusMoveCompletedPublisher = bonusMoveCompletedPublisher;
            _logger = logService.CreateLogger<TurnController>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            _subscriptions = bag.Build();
            
            _logger.Info("TurnController created");
        }

        public async UniTask ExecuteMoveAsync(Figure actor, GridPosition from, GridPosition to)
        {
            await ExecuteTurnAsync(actor, from, to);
        }

        public async UniTask ExecuteAttackAsync(Figure actor, GridPosition from, GridPosition to)
        {
            await ExecuteTurnAsync(actor, from, to);
        }

        private async UniTask ExecuteTurnAsync(Figure actor, GridPosition from, GridPosition to)
        {
            BoardGrid grid = GetCurrentGrid();
            if (grid == null)
            {
                _logger.Error("Cannot execute turn: no grid available");
                return;
            }

            if (actor == null)
            {
                _logger.Error("Cannot execute turn: actor is null");
                return;
            }

            _logger.Info($"Executing turn for {actor.Id}: ({from.Row},{from.Column}) -> ({to.Row},{to.Column})");

            // Acquire interaction lock for the entire turn duration
            using (IDisposable lockHandle = _interactionLock.Acquire("turn-execution"))
            {
                try
                {
                    // Execute the turn via TurnExecutor (handles visuals internally)
                    TurnExecutionResult result = await _turnExecutor.ExecuteAsync(actor, from, to, grid);

                    if (!result.Success)
                    {
                        _logger.Debug($"Turn execution failed for {actor}");
                        return;
                    }

                    _logger.Info($"Turn executed. Final pos: ({result.ActorFinalPosition.Row},{result.ActorFinalPosition.Column}), " +
                                 $"BonusMove: {(result.BonusMoveDistance.HasValue ? result.BonusMoveDistance.Value.ToString() : "none")}");

                    // Handle bonus move if granted
                    if (result.BonusMoveDistance.HasValue && result.BonusMoveDistance.Value > 0)
                    {
                        await HandleBonusMoveAsync(actor, result.ActorFinalPosition, result.BonusMoveDistance.Value, grid);
                    }

                    // End the turn
                    _turnSystem.EndTurn();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Turn execution error: {ex.Message}");
                    throw;
                }
            }
        }

        private async UniTask HandleBonusMoveAsync(Figure actor, GridPosition from, int maxDistance, BoardGrid grid)
        {
            _logger.Info($"Starting bonus move for {actor.Id} from ({from.Row},{from.Column}), max distance: {maxDistance}");

            _isInBonusMoveMode = true;
            _bonusMoveActor = actor;

            try
            {
                // Initialize bonus move state (no click subscription in BonusMoveController)
                _bonusMoveController.Start(actor, from, maxDistance, grid);

                // Publish start message for StageService to show highlights
                _bonusMoveStartedPublisher.Publish(new BonusMoveStartedMessage(actor));

                // Wait for valid click or cancellation
                bool completed = false;
                while (!completed && _bonusMoveController.IsActive)
                {
                    // Wait for next click
                    _pendingClickTcs = new UniTaskCompletionSource<GridPosition>();
                    GridPosition clickedPosition;
                    
                    try
                    {
                        clickedPosition = await _pendingClickTcs.Task;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Debug("Bonus move cancelled via cancellation");
                        _bonusMoveController.Cancel();
                        break;
                    }

                    // Try to execute the bonus move (domain only)
                    if (_bonusMoveController.TryExecute(clickedPosition))
                    {
                        // Domain succeeded - now play visual via VisualPipeline
                        await PlayBonusMoveVisualAsync(actor, clickedPosition);
                        
                        completed = true;
                        _logger.Info($"Bonus move completed to ({clickedPosition.Row},{clickedPosition.Column})");
                    }
                    else
                    {
                        _logger.Debug($"Bonus move rejected for position ({clickedPosition.Row},{clickedPosition.Column})");
                    }
                }

                // Publish completion message for StageService to clear highlights
                _bonusMoveCompletedPublisher.Publish(new BonusMoveCompletedMessage(actor));
            }
            finally
            {
                _isInBonusMoveMode = false;
                _bonusMoveActor = null;
                _pendingClickTcs = null;
            }
        }

        /// <summary>
        /// Plays the bonus move animation via VisualPipeline.
        /// Same pattern as MoveStep uses.
        /// </summary>
        private async UniTask PlayBonusMoveVisualAsync(Figure actor, GridPosition to)
        {
            using VisualScope scope = _visualPipeline.BeginScope();
            scope.Enqueue(new MoveCommand(new MoveVisualContext(actor.Id, to)));
            await scope.PlayAsync();
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            // Only handle clicks during bonus move mode
            if (!_isInBonusMoveMode)
                return;

            BoardGrid grid = GetCurrentGrid();
            if (grid == null || !grid.IsInside(message.Position))
                return;

            _logger.Debug($"Bonus move click received: ({message.Position.Row},{message.Position.Column})");

            // Complete the pending task with the clicked position
            if (_pendingClickTcs != null)
            {
                _pendingClickTcs.TrySetResult(message.Position);
            }
        }

        private BoardGrid GetCurrentGrid()
        {
            return _runHolder.Current?.CurrentStage?.Grid;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            if (_pendingClickTcs != null)
            {
                _pendingClickTcs.TrySetCanceled();
            }
        }
    }
}
