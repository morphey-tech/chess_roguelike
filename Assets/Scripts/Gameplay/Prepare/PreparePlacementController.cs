using Project.Core.Core.Combat;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Handles placement and unplace (return to hand) transactions.
    /// </summary>
    public sealed class PreparePlacementController
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly BoardCapacityService _capacityService;
        private readonly IPreparePresenter _preparePresenter;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IFigureRegistry _figureRegistry;
        private readonly IGameUiService _uiService;
        private readonly IPublisher<PrepareSelectionChangedMessage> _selectionChangedPublisher;
        private readonly IPublisher<FigureBoardRemovedMessage> _figureRemovedPublisher;
        private readonly ILogger<PreparePlacementController> _logger;
        private bool _isPlacing;

        [Inject]
        private PreparePlacementController(
            FigureSpawnService figureSpawnService,
            IPreparePresenter preparePresenter,
            BoardCapacityService capacityService,
            IFigurePresenter figurePresenter,
            IFigureRegistry figureRegistry,
            IGameUiService uiService,
            IPublisher<PrepareSelectionChangedMessage> selectionChangedPublisher,
            IPublisher<FigureBoardRemovedMessage> figureRemovedPublisher,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _preparePresenter = preparePresenter;
            _capacityService = capacityService;
            _figurePresenter = figurePresenter;
            _figureRegistry = figureRegistry;
            _uiService = uiService;
            _selectionChangedPublisher = selectionChangedPublisher;
            _figureRemovedPublisher = figureRemovedPublisher;
            _logger = logService.CreateLogger<PreparePlacementController>();
        }

        public async UniTask<PreparePlacementResult> PlaceSelectedAsync(
            PrepareContext context,
            GridPosition pos,
            CancellationToken cancellationToken)
        {
            if (_isPlacing)
            {
                return PreparePlacementResult.Ignored;
            }

            _isPlacing = true;
            try
            {
                if (!context.State.IsActive)
                {
                    return PreparePlacementResult.Ignored;
                }

                FigureState? state = context.GetSelectedFigure();
                if (state == null)
                {
                    return PreparePlacementResult.Ignored;
                }

                bool canSpawn = await _capacityService.CanSpawnByTypeAsync(state.TypeId);
                if (!canSpawn)
                {
                    _uiService.ShowWarning("No board capacity");
                    return new PreparePlacementResult(true, false, false);
                }

                context.PreviousSelectedId = null;

                PreparePlacementTransaction transaction = new(
                    context.State,
                    _preparePresenter,
                    _figureSpawnService,
                    context.RunState,
                    context.Grid,
                    _logger);

                bool success = await transaction.ExecuteAsync(state, pos, cancellationToken);
                if (success)
                {
                    context.AvailablePlacementPositions.Remove(pos);
                    return new PreparePlacementResult(true, true, context.State.IsCompleted);
                }

                if (context.Rules.CanPlace(pos))
                {
                    context.AvailablePlacementPositions.Add(pos);
                }

                return new PreparePlacementResult(true, false, false);
            }
            finally
            {
                _isPlacing = false;
            }
        }

        /// <summary>
        /// Removes a figure from the board and returns it to hand (click on placed figure).
        /// </summary>
        public async UniTask<bool> UnplaceAtAsync(PrepareContext context, GridPosition pos, CancellationToken cancellationToken)
        {
            if (_isPlacing)
            {
                return false;
            }

            _isPlacing = true;
            try
            {
                if (!context.State.IsActive || !context.IsInputReady)
                {
                    return false;
                }

                FigureState? figureState = context.RunState.GetFigureAtPosition(pos);
                if (figureState == null)
                {
                    return false;
                }

                BoardCell cell = context.Grid.GetBoardCell(pos);
                Figure? figure = cell.OccupiedBy;
                if (figure is not { Team: Team.Player })
                {
                    return false;
                }

                context.State.ClearSelection();
                if (context.PreviousSelectedId != null)
                {
                    _selectionChangedPublisher.Publish(new PrepareSelectionChangedMessage(context.PreviousSelectedId, false));
                }
                context.PreviousSelectedId = null;

                int figureIdForView = figure.Id;
                context.RunState.ReturnToHand(figureState.Id);
                context.Grid.RemoveFigure(figure);
                _figureRemovedPublisher.Publish(new FigureBoardRemovedMessage(figure.Id, figure.Team));
                _figureRegistry.Unregister(figure);

                await _figurePresenter.RemoveFigureAsync(figureIdForView).AttachExternalCancellation(cancellationToken);
                await _capacityService.RecalculateFromBoard(context.Grid.GetFiguresByTeam(Team.Player));

                context.State.Restore(figureState.Id);
                await _preparePresenter.RestoreFigureAsync(figureState.Id).AttachExternalCancellation(cancellationToken);

                if (context.Rules.CanPlace(pos))
                {
                    context.AvailablePlacementPositions.Add(pos);
                }

                _logger.Info($"Unplaced {figureState.TypeId} from ({pos.Row}, {pos.Column})");
                return true;
            }
            finally
            {
                _isPlacing = false;
            }
        }
    }
}
