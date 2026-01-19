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
    public sealed class PrepareService : IDisposable
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly IPreparePresenter _preparePresenter;
        private readonly IPublisher<PreparePhaseCompletedMessage> _completedPublisher;
        private readonly ILogger<PrepareService> _logger;
        private readonly IDisposable _subscriptions;

        private PrepareState? _state;
        private IPreparePlacementRules? _rules;
        private PlayerRunStateModel _runState;
        private BoardGrid _grid;
        private string? _previousSelectedId;

        [Inject]
        public PrepareService(
            FigureSpawnService figureSpawnService,
            IPreparePresenter preparePresenter,
            ISubscriber<HandFigureClickedMessage> handFigureClickedSubscriber,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            IPublisher<PreparePhaseCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _preparePresenter = preparePresenter;
            _completedPublisher = completedPublisher;
            _logger = logService.CreateLogger<PrepareService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            handFigureClickedSubscriber.Subscribe(OnHandFigureClicked).AddTo(bag);
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            cancelSubscriber.Subscribe(_ => OnCancel()).AddTo(bag);
            _subscriptions = bag.Build();

            _logger.Info("PrepareService created");
        }

        public async UniTask Start(PlayerRunStateModel runState, BoardGrid grid, IPreparePlacementRules rules)
        {
            _runState = runState;
            _grid = grid;
            _rules = rules;

            _state = new PrepareState(runState.FiguresInHand);
            _state.Start();

            _logger.Info($"Prepare started: {runState.FiguresInHand.Count} figures in hand");

            await SpawnPrepareZoneAsync();
        }

        private async UniTask SpawnPrepareZoneAsync()
        {
            var figures = _runState.FiguresInHand;
            int totalCount = figures.Count;
            
            for (int i = 0; i < totalCount; i++)
            {
                FigureState fig = figures[i];
                await _preparePresenter.CreateSlotWithFigureAsync(i, totalCount, fig.Id, fig.TypeId);
            }

            _logger.Info($"Spawned {totalCount} figures in prepare zone");

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
            _preparePresenter.Clear();
            _logger.Info("Prepare completed");
            _completedPublisher.Publish(new PreparePhaseCompletedMessage(_runState.FiguresOnBoard.Count));

            _state = null;
            _rules = null;
            _previousSelectedId = null;
        }

        private void OnHandFigureClicked(HandFigureClickedMessage message)
        {
            if (_state is not { IsActive: true })
            {
                return;
            }

            // Deselect previous
            if (_previousSelectedId != null)
            {
                _preparePresenter.SetSelected(_previousSelectedId, false);
            }

            _state.Select(message.FigureId);
            _previousSelectedId = message.FigureId;
            _preparePresenter.SetSelected(message.FigureId, true);

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

            string figureId = state.Id;

            Figure figure = await _figureSpawnService.SpawnAsync(_grid, pos, state.TypeId, Team.Player);
            if (figure == null)
            {
                _logger.Error("Failed to spawn figure");
                return;
            }

            _preparePresenter.RemoveFigure(figureId);
            _previousSelectedId = null;

            _runState.PlaceOnBoard(figureId, pos);
            
            _state.OnPlaced(figureId);
            _logger.Info($"Placed {state.TypeId} at ({pos.Row}, {pos.Column})");

            if (_state.IsCompleted)
            {
                _logger.Info("All figures placed");
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

            if (_previousSelectedId != null)
            {
                _preparePresenter.SetSelected(_previousSelectedId, false);
            }

            _logger.Debug("Selection cancelled");
            _state.ClearSelection();
            _previousSelectedId = null;
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
