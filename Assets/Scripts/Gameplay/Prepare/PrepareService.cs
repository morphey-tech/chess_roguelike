using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Save.Models;
using VContainer;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Orchestrates the Prepare phase.
    /// - Subscribed to input
    /// - Holds temporary PrepareState
    /// - Updates RunState (single source of truth)
    /// - No Unity dependencies
    /// </summary>
    public sealed class PrepareService : IDisposable
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly IPublisher<PreparePhaseCompletedMessage> _completedPublisher;
        private readonly ILogger<PrepareService> _logger;
        private readonly IDisposable _subscriptions;

        private PrepareState? _state;
        private IPreparePlacementRules? _rules;
        private PlayerRunStateModel _runState;
        private BoardGrid _grid;

        [Inject]
        public PrepareService(
            FigureSpawnService figureSpawnService,
            ISubscriber<HandFigureClickedMessage> handFigureClickedSubscriber,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            IPublisher<PreparePhaseCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _completedPublisher = completedPublisher;
            _logger = logService.CreateLogger<PrepareService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            handFigureClickedSubscriber.Subscribe(OnHandFigureClicked).AddTo(bag);
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            cancelSubscriber.Subscribe(_ => OnCancel()).AddTo(bag);
            _subscriptions = bag.Build();

            _logger.Info("PrepareService created");
        }

        public void Start(PlayerRunStateModel runState, BoardGrid grid, IPreparePlacementRules rules)
        {
            _runState = runState;
            _grid = grid;
            _rules = rules;

            // Create temporary phase state from units in hand
            _state = new PrepareState(runState.FiguresInHand);
            _state.Start();

            _logger.Info($"Prepare started: {runState.FiguresInHand.Count} figures in hand");

            // If no figures to place, complete immediately
            if (_state.IsCompleted)
            {
                _logger.Info("No figures to place, completing immediately");
                Complete();
            }
        }

        private void Complete()
        {
            if (_state is not { IsActive: true })
            {
                return;
            }

            _state.Complete();
            _logger.Info("Prepare completed");
            _completedPublisher.Publish(new PreparePhaseCompletedMessage(_runState.FiguresOnBoard.Count));

            // Clear phase state
            _state = null;
            _rules = null;
        }

        private void OnHandFigureClicked(HandFigureClickedMessage message)
        {
            if (_state is not { IsActive: true })
            {
                return;
            }

            _state.Select(message.FigureId);
            FigureState? selected = _state.GetSelectedFigure(_runState);
            if (selected != null)
            {
                _logger.Debug($"Selected: {selected.TypeId} (id={selected.Id})");
            }
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            if (_state is not { IsActive: true })
            {
                return;
            }
            if (_state.SelectedFigureId == null)
            {
                return;
            }

            GridPosition pos = message.Position;
            if (!_rules.CanPlace(pos))
            {
                _logger.Debug($"Invalid placement: ({pos.Row}, {pos.Column})");
                return;
            }

            PlaceSelectedAsync(pos).Forget();
        }

        private async UniTaskVoid PlaceSelectedAsync(GridPosition pos)
        {
            FigureState? state = _state.GetSelectedFigure(_runState);
            if (state == null)
            {
                return;
            }

            Figure figure = await _figureSpawnService.SpawnAsync(_grid, pos, state.TypeId, Team.Player);
            if (figure == null)
            {
                _logger.Error("Failed to spawn figure");
                return;
            }

            // Update RunState (single source of truth)
            _runState.PlaceOnBoard(state.Id, pos);
            
            // Update phase state
            _state.OnPlaced(state.Id);
            _logger.Info($"Placed {state.TypeId} at ({pos.Row}, {pos.Column})");

            if (_state.IsCompleted)
            {
                _logger.Info("All units placed");
                Complete();
            }
        }

        private void OnCancel()
        {
            if (_state is not { IsActive: true })
            {
                return;
            }
            if (_state.SelectedFigureId == null)
            {
                return;
            }
            _logger.Debug("Selection cancelled");
            _state.ClearSelection();
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
