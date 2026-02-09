using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;
using Project.Gameplay.UI;
using Project.Gameplay.Components;
using VContainer;

namespace Project.Gameplay.Gameplay.Prepare
{
    public sealed class PrepareService : IDisposable
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly IPreparePresenter _preparePresenter;
        private readonly VisualPipeline _visualPipeline;
        private readonly IPublisher<PreparePhaseCompletedMessage> _completedPublisher;
        private readonly ILogger<PrepareService> _logger;
        private readonly IDisposable _subscriptions;

        private PrepareState? _state;
        private IPreparePlacementRules? _rules;
        private PlayerRunStateModel? _runState;
        private BoardGrid? _grid;
        private string? _previousSelectedId;
        private bool _isPlacing;
        private HashSet<GridPosition>? _availablePlacementPositions;

        [Inject]
        private PrepareService(
            FigureSpawnService figureSpawnService,
            IPreparePresenter preparePresenter,
            VisualPipeline visualPipeline,
            ISubscriber<HandFigureClickedMessage> handFigureClickedSubscriber,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            IPublisher<PreparePhaseCompletedMessage> completedPublisher,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _preparePresenter = preparePresenter;
            _visualPipeline = visualPipeline;
            _completedPublisher = completedPublisher;
            _logger = logService.CreateLogger<PrepareService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            handFigureClickedSubscriber.Subscribe(OnHandFigureClicked).AddTo(bag);
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            cancelSubscriber.Subscribe(_ => OnCancel()).AddTo(bag);
            _subscriptions = bag.Build();
        }

        public async UniTask Start(PlayerRunStateModel runState, BoardGrid grid, IPreparePlacementRules rules)
        {
            _runState = runState;
            _grid = grid;
            _rules = rules;

            _state = new PrepareState(runState.FiguresInHand);
            _state.Start();

            // Preload в фоне — не блокируем появление prepare-зоны (PreloadConfigsAsync может тянуть 2–3 сек из-за TurnPatternFactory)
            _figureSpawnService.PreloadConfigsAsync().Forget();
            await SpawnPrepareZoneAsync();
            BuildPlacementCache();
            UpdatePlacementHighlights();
            await ShowHintWindow();
        }

        private async UniTask SpawnPrepareZoneAsync()
        {
            List<PrepareZoneFigureData> figureDataList = new List<PrepareZoneFigureData>();
            foreach (FigureState fig in _runState!.FiguresInHand)
            {
                figureDataList.Add(new PrepareZoneFigureData(fig.Id, fig.TypeId));
            }

            using (VisualScope scope = _visualPipeline.BeginScope())
            {
                scope.Enqueue(new SpawnPrepareZoneCommand(figureDataList));
                await scope.PlayAsync();
            }

            if (_state?.IsCompleted == true)
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
            ClearPlacementHighlights();
            _completedPublisher.Publish(new PreparePhaseCompletedMessage(_runState!.FiguresOnBoard.Count));

            _state = null;
            _rules = null;
            _previousSelectedId = null;
            _availablePlacementPositions = null;
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

            FigureState? selected = _state.GetSelectedFigure(_runState!);
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

        private async UniTask PlaceSelectedAsync(GridPosition pos)
        {
            if (_isPlacing)
                return;

            _isPlacing = true;
            try
            {
                if (_state is not { IsActive: true })
                    return;

                FigureState? state = _state.GetSelectedFigure(_runState!);
                if (state == null)
                    return;

                _previousSelectedId = null;

                var transaction = new PreparePlacementTransaction(
                    _state,
                    _preparePresenter,
                    _figureSpawnService,
                    _runState!,
                    _grid!,
                    _logger);

                bool success = await transaction.ExecuteAsync(state, pos);
                if (success)
                {
                    _availablePlacementPositions?.Remove(pos);
                    UpdatePlacementHighlights();

                    if (_state?.IsCompleted == true)
                    {
                        _logger.Info("All figures placed");
                        Complete();
                    }
                }
                else
                {
                    if (_availablePlacementPositions != null && _rules != null && _rules.CanPlace(pos))
                        _availablePlacementPositions.Add(pos);
                    UpdatePlacementHighlights();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("PlaceSelectedAsync failed", ex);
            }
            finally
            {
                _isPlacing = false;
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

        private void UpdatePlacementHighlights()
        {
            if (_grid == null || _rules == null)
            {
                return;
            }

            foreach (BoardCell cell in _grid.AllCells())
            {
                GridPosition pos = cell.Position;
                cell.Del<AttackHighlightTag>();
                bool canPlace = _availablePlacementPositions?
                    .Contains(pos) ?? _rules.CanPlace(pos);
                if (canPlace)
                {
                    cell.EnsureComponent(new HighlightTag());
                }
                else
                {
                    cell.Del<HighlightTag>();
                }
            }
        }

        private async UniTask ShowHintWindow()
        {
            TurnWindow? wnd = await Core.Window.UI.ShowAsync<TurnWindow>();
            wnd?.SetPreparePhase();
        }

        private void ClearPlacementHighlights()
        {
            if (_grid == null)
            {
                return;
            }
            foreach (BoardCell cell in _grid.AllCells())
            {
                cell.Del<HighlightTag>();
                cell.Del<AttackHighlightTag>();
            }
        }

        private void BuildPlacementCache()
        {
            if (_grid == null || _rules == null)
                return;

            _availablePlacementPositions = new HashSet<GridPosition>();
            foreach (BoardCell cell in _grid.AllCells())
            {
                GridPosition pos = cell.Position;
                if (_rules.CanPlace(pos))
                    _availablePlacementPositions.Add(pos);
            }
        }

        public void Reset()
        {
            _state = null;
            _rules = null;
            _previousSelectedId = null;
            _availablePlacementPositions = null;
            _isPlacing = false;

            if (_grid != null)
            {
                ClearPlacementHighlights();
            }

            _preparePresenter.Clear();
            _runState = null;
            _grid = null;

            _logger.Info("PrepareService reset");
        }
    }
}
