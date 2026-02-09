using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Turn;
using VContainer;

namespace Project.Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Central controller for player interaction.
    /// Owns selection state, receives click events, and delegates execution to TurnController.
    /// 
    /// Does NOT:
    /// - Know about bonus moves (handled via IInteractionLock)
    /// - Reference VisualPipeline
    /// - Execute turns directly
    /// </summary>
    public sealed class InteractionController : IDisposable
    {
        private readonly IInteractionLock _interactionLock;
        private readonly IClickIntentResolver _intentResolver;
        private readonly ITurnController _turnController;
        private readonly TurnSystem _turnSystem;
        private readonly RunHolder _runHolder;
        private readonly IPublisher<FigureSelectedMessage> _figureSelectedPublisher;
        private readonly IPublisher<FigureDeselectedMessage> _figureDeselectedPublisher;
        private readonly ILogger<InteractionController> _logger;
        private readonly IDisposable _subscriptions;

        private BoardGrid? _grid;

        public bool IsActive { get; private set; }
        public Figure? SelectedFigure { get; private set; }
        public GridPosition? SelectedPosition { get; private set; }
        public bool HasSelection => SelectedFigure != null;

        [Inject]
        public InteractionController(
            IInteractionLock interactionLock,
            IClickIntentResolver intentResolver,
            ITurnController turnController,
            TurnSystem turnSystem,
            RunHolder runHolder,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            ISubscriber<TurnChangedMessage> turnChangedSubscriber,
            IPublisher<FigureSelectedMessage> figureSelectedPublisher,
            IPublisher<FigureDeselectedMessage> figureDeselectedPublisher,
            ILogService logService)
        {
            _interactionLock = interactionLock;
            _intentResolver = intentResolver;
            _turnController = turnController;
            _turnSystem = turnSystem;
            _runHolder = runHolder;
            _figureSelectedPublisher = figureSelectedPublisher;
            _figureDeselectedPublisher = figureDeselectedPublisher;
            _logger = logService.CreateLogger<InteractionController>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            cancelSubscriber.Subscribe(_ => ClearSelection()).AddTo(bag);
            turnChangedSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            _subscriptions = bag.Build();
        }

        /// <summary>
        /// Configures the controller with the active grid.
        /// </summary>
        public void Configure(BoardGrid grid)
        {
            _grid = grid;
            IsActive = true;
            ClearSelection();
            _logger.Info("InteractionController activated");
        }

        /// <summary>
        /// Deactivates the controller.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            ClearSelection();
            _logger.Info("InteractionController deactivated");
        }

        private void OnTurnChanged(TurnChangedMessage message)
        {
            ClearSelection();
            _logger.Debug($"Turn changed to {message.CurrentTeam}, selection cleared");
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            if (!IsActive)
            {
                _logger.Debug("Click ignored: controller not active");
                return;
            }

            if (_interactionLock.IsLocked)
            {
                _logger.Debug("Click ignored: interaction locked");
                return;
            }

            BoardGrid? grid = GetCurrentGrid();
            if (grid == null)
            {
                _logger.Warning("Click ignored: no grid available");
                return;
            }

            InteractionContext context = new(
                grid,
                message.Position,
                SelectedFigure,
                SelectedPosition,
                _turnSystem.CurrentTeam
            );

            ClickIntent intent = _intentResolver.Resolve(context);
            _logger.Debug($"Cell ({message.Position.Row},{message.Position.Column}) clicked, intent: {intent.Type}");

            HandleIntent(intent);
        }

        private void HandleIntent(ClickIntent intent)
        {
            switch (intent.Type)
            {
                case CellClickIntent.SelectFigure:
                    HandleSelectFigure(intent);
                    break;

                case CellClickIntent.Move:
                    HandleMove(intent);
                    break;

                case CellClickIntent.Attack:
                    HandleAttack(intent);
                    break;

                case CellClickIntent.None:
                default:
                    _logger.Debug("Intent: None - no action taken");
                    break;
            }
        }

        private void HandleSelectFigure(ClickIntent intent)
        {
            // If we already have a selection, deselect first
            if (SelectedFigure != null)
            {
                Deselect(SelectedFigure);
            }

            Select(intent.TargetFigure, intent.To.Value);
        }

        private void HandleMove(ClickIntent intent)
        {
            Figure actor = SelectedFigure;
            GridPosition from = intent.From.Value;
            GridPosition to = intent.To.Value;

            // Deselect and clear before executing
            if (SelectedFigure != null)
            {
                Deselect(SelectedFigure);
            }
            ClearSelectionInternal();

            // Delegate to TurnController (async, fire-and-forget from our perspective)
            _turnController.ExecuteMoveAsync(actor, from, to).Forget();
        }

        private void HandleAttack(ClickIntent intent)
        {
            Figure actor = SelectedFigure;
            GridPosition from = intent.From.Value;
            GridPosition to = intent.To.Value;

            // Deselect and clear before executing
            if (SelectedFigure != null)
            {
                Deselect(SelectedFigure);
            }
            ClearSelectionInternal();

            // Delegate to TurnController (async, fire-and-forget from our perspective)
            _turnController.ExecuteAttackAsync(actor, from, to).Forget();
        }

        private void Select(Figure figure, GridPosition position)
        {
            SelectedFigure = figure;
            SelectedPosition = position;
            _logger.Debug($"Selected {figure} at ({position.Row},{position.Column})");
            _figureSelectedPublisher.Publish(new FigureSelectedMessage(figure, position));
        }

        private void Deselect(Figure figure)
        {
            _figureDeselectedPublisher.Publish(new FigureDeselectedMessage(figure));
        }

        /// <summary>
        /// Clears the current selection and publishes appropriate messages.
        /// </summary>
        public void ClearSelection()
        {
            if (SelectedFigure != null)
            {
                Deselect(SelectedFigure);
                _logger.Debug("Selection cleared");
            }
            ClearSelectionInternal();
            _figureSelectedPublisher.Publish(new FigureSelectedMessage(null, default));
        }

        private void ClearSelectionInternal()
        {
            SelectedFigure = null;
            SelectedPosition = null;
        }

        private BoardGrid? GetCurrentGrid()
        {
            return _grid ?? _runHolder.Current?.CurrentStage?.Grid;
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
