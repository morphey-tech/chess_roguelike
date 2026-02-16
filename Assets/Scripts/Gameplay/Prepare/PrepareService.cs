using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Async;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;
using Project.Gameplay.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.Prepare
{
    public sealed class PrepareService : IDisposable
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly VisualPipeline _visualPipeline;
        private readonly PreparePlacementController _placementController;
        private readonly PrepareHighlightService _highlightService;
        private readonly IGameUiService _uiService;
        private readonly IPublisher<PreparePhaseCompletedMessage> _completedPublisher;
        private readonly IPublisher<PrepareSelectionChangedMessage> _selectionChangedPublisher;
        private readonly IPublisher<PrepareVisualResetMessage> _visualResetPublisher;
        private readonly ILogger<PrepareService> _logger;

        private PrepareContext? _context;
        private CancellationTokenSource? _sessionCts;

        [Inject]
        private PrepareService(
            FigureSpawnService figureSpawnService,
            VisualPipeline visualPipeline,
            PreparePlacementController placementController,
            PrepareHighlightService highlightService,
            IGameUiService uiService,
            IPublisher<PreparePhaseCompletedMessage> completedPublisher,
            IPublisher<PrepareSelectionChangedMessage> selectionChangedPublisher,
            IPublisher<PrepareVisualResetMessage> visualResetPublisher,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _visualPipeline = visualPipeline;
            _placementController = placementController;
            _highlightService = highlightService;
            _uiService = uiService;
            _completedPublisher = completedPublisher;
            _selectionChangedPublisher = selectionChangedPublisher;
            _visualResetPublisher = visualResetPublisher;
            _logger = logService.CreateLogger<PrepareService>();
        }

        public async UniTask Start(PlayerRunStateModel runState, BoardGrid grid, IPreparePlacementRules rules)
        {
            Reset();

            _sessionCts = new CancellationTokenSource();
            var context = new PrepareContext(
                runState,
                grid,
                rules,
                new PrepareState(runState.FiguresInHand),
                _sessionCts.Token);
            context.State.Start();
            _context = context;

            // Preload in background, but under session lifetime.
            _figureSpawnService
                .PreloadConfigsAsync()
                .AttachExternalCancellation(context.CancellationToken)
                .ForgetLogged(_logger, "Prepare preload failed", context.CancellationToken);

            try
            {
                await SpawnPrepareZoneAsync(context);
                if (_context != context)
                    return;

                _highlightService.BuildPlacementCache(context);
                _highlightService.ApplyAll(context);

                await _uiService.ShowWorldUiAsync().AttachExternalCancellation(context.CancellationToken);
                await _uiService.ShowPreparePhaseAsync().AttachExternalCancellation(context.CancellationToken);
                context.IsInputReady = true;
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                _logger.Debug("Prepare start cancelled");
            }
        }

        private async UniTask SpawnPrepareZoneAsync(PrepareContext context)
        {
            List<PrepareZoneFigureData> figureDataList = new List<PrepareZoneFigureData>();
            foreach (FigureState fig in context.RunState.FiguresInHand)
            {
                figureDataList.Add(new PrepareZoneFigureData(fig.Id, fig.TypeId));
            }

            using (VisualScope scope = _visualPipeline.BeginScope())
            {
                scope.Enqueue(new SpawnPrepareZoneCommand(figureDataList));
                await scope.PlayAsync().AttachExternalCancellation(context.CancellationToken);
            }

            if (_context == context && context.State.IsCompleted)
            {
                _logger.Info("No figures to place, completing immediately");
                Complete(context);
            }
        }

        /// <summary>
        /// Called when user requests to finish prepare (e.g. "Finish" button) or when all figures are placed.
        /// </summary>
        public void RequestCompletePrepare()
        {
            PrepareContext? context = _context;
            if (context == null || !context.State.IsActive)
                return;

            Complete(context);
        }

        private void Complete(PrepareContext context)
        {
            if (_context != context || !context.State.IsActive)
            {
                return;
            }

            context.State.Complete();
            _completedPublisher.Publish(new PreparePhaseCompletedMessage(context.RunState.FiguresOnBoard.Count));
            EndSession(clearVisuals: true);
        }

        public void HandleHandFigureClicked(HandFigureClickedMessage message)
        {
            PrepareContext? context = _context;
            if (context == null || !context.State.IsActive || !context.IsInputReady)
            {
                return;
            }

            if (context.PreviousSelectedId != null)
            {
                _selectionChangedPublisher.Publish(new PrepareSelectionChangedMessage(context.PreviousSelectedId, false));
            }

            context.State.Select(message.FigureId);
            if (context.State.SelectedFigureId != message.FigureId)
                return;

            context.PreviousSelectedId = message.FigureId;
            _selectionChangedPublisher.Publish(new PrepareSelectionChangedMessage(message.FigureId, true));
            FigureState? selected = context.GetSelectedFigure();
            if (selected != null)
            {
                _logger.Debug($"Selected: {selected.TypeId} (id={selected.Id})");
            }
        }

        public void HandleCellClicked(CellClickedMessage message)
        {
            PrepareContext? context = _context;
            if (context == null || !context.State.IsActive || !context.IsInputReady)
            {
                return;
            }

            GridPosition pos = message.Position;

            // Click on already placed (our) figure — return to hand
            if (context.RunState.GetFigureAtPosition(pos) != null)
            {
                HandleUnplaceAsync(context, pos)
                    .AttachExternalCancellation(context.CancellationToken)
                    .ForgetLogged(_logger, "Prepare unplace failed", context.CancellationToken);
                return;
            }

            if (context.State.SelectedFigureId == null)
            {
                return;
            }

            if (!context.Rules.CanPlace(pos))
            {
                _logger.Debug($"Invalid placement: ({pos.Row}, {pos.Column})");
                return;
            }

            HandlePlacementAsync(context, pos)
                .AttachExternalCancellation(context.CancellationToken)
                .ForgetLogged(_logger, "Prepare placement failed", context.CancellationToken);
        }

        private async UniTask HandlePlacementAsync(PrepareContext context, GridPosition pos)
        {
            PreparePlacementResult result = await _placementController.PlaceSelectedAsync(context, pos, context.CancellationToken);
            if (!result.Processed || _context != context)
                return;

            _highlightService.ApplyDirty(context, new[] { pos });

            if (result.Completed)
            {
                _logger.Info("All figures placed");
                Complete(context);
            }
        }

        private async UniTask HandleUnplaceAsync(PrepareContext context, GridPosition pos)
        {
            bool success = await _placementController.UnplaceAtAsync(context, pos, context.CancellationToken);
            if (!success || _context != context)
                return;

            _highlightService.ApplyDirty(context, new[] { pos });
        }

        public void HandleCancelRequested()
        {
            PrepareContext? context = _context;
            if (context == null || !context.State.IsActive
                                || !context.IsInputReady 
                                || context.State.SelectedFigureId == null)
            {
                return;
            }

            if (context.PreviousSelectedId != null)
            {
                _selectionChangedPublisher.Publish(new 
                    PrepareSelectionChangedMessage(context.PreviousSelectedId, false));
            }

            _logger.Debug("Selection cancelled");
            context.State.ClearSelection();
            context.PreviousSelectedId = null;
        }

        public void Reset()
        {
            EndSession(clearVisuals: true);
            _logger.Info("PrepareService reset");
        }

        public void Dispose()
        {
            EndSession(clearVisuals: false);
        }

        private void EndSession(bool clearVisuals)
        {
            PrepareContext? context = _context;
            if (context != null)
            {
                context.IsInputReady = false;
                context.PreviousSelectedId = null;
                _highlightService.Clear(context.Grid);
            }

            _context = null;
            CancelSession();

            if (clearVisuals)
                _visualResetPublisher.Publish(new PrepareVisualResetMessage());
        }

        private void CancelSession()
        {
            if (_sessionCts == null)
            {
                return;
            }
            if (!_sessionCts.IsCancellationRequested)
            {
                _sessionCts.Cancel();
            }
            _sessionCts.Dispose();
            _sessionCts = null;
        }
    }
}
