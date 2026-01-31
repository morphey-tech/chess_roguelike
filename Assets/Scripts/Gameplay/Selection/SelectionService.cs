using System;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Turn;
using VContainer;

namespace Project.Gameplay.Gameplay.Selection
{
    public class SelectionService : IDisposable
    {
        private readonly TurnSystem _turnSystem;
        private readonly IPublisher<FigureSelectedMessage> _figureSelectedPublisher;
        private readonly IPublisher<MoveRequestedMessage> _moveRequestedPublisher;
        private readonly IPublisher<AttackRequestedMessage> _attackRequestedPublisher;
        private readonly ILogger<SelectionService> _logger;
        private readonly IDisposable _subscriptions;

        private BoardGrid _grid;
        private BoardCell? _selectedCell;
        private bool _isActive;

        public bool IsActive => _isActive;
        public bool HasSelection => _selectedCell != null;
        public Figure SelectedFigure => _selectedCell?.OccupiedBy;

        [Inject]
        private SelectionService(
            TurnSystem turnSystem,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            IPublisher<FigureSelectedMessage> figureSelectedPublisher,
            IPublisher<MoveRequestedMessage> moveRequestedPublisher,
            IPublisher<AttackRequestedMessage> attackRequestedPublisher,
            ILogService logService)
        {
            _turnSystem = turnSystem;
            _figureSelectedPublisher = figureSelectedPublisher;
            _moveRequestedPublisher = moveRequestedPublisher;
            _attackRequestedPublisher = attackRequestedPublisher;
            _logger = logService.CreateLogger<SelectionService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            cancelSubscriber.Subscribe(_ => ClearSelection()).AddTo(bag);
            _subscriptions = bag.Build();
            
            _logger.Info("SelectionService created");
        }

        public void Configure(BoardGrid grid)
        {
            _grid = grid;
            _isActive = true;
            ClearSelection();
            _logger.Info("SelectionService activated");
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            if (!_isActive)
                return;

            GridPosition position = message.Position;

            if (!_grid.IsInside(position))
                return;

            BoardCell clickedCell = _grid.GetBoardCell(position);
            CellClickIntent intent = ResolveIntent(clickedCell);

            _logger.Debug($"Cell ({position.Row},{position.Column}) clicked, intent: {intent}");

            switch (intent)
            {
                case CellClickIntent.SelectFigure:
                    Select(clickedCell);
                    break;

                case CellClickIntent.Move:
                    RequestMove(_selectedCell.Position, position);
                    ClearSelection();
                    break;

                case CellClickIntent.Attack:
                    RequestAttack(_selectedCell.Position, position);
                    ClearSelection();
                    break;

                case CellClickIntent.None:
                default:
                    _logger.Debug("Click ignored: no valid intent");
                    break;
            }
        }

        private CellClickIntent ResolveIntent(BoardCell cell)
        {
            if (!HasSelection)
            {
                return IsFriendly(cell) ? CellClickIntent.SelectFigure : CellClickIntent.None;
            }

            if (cell.IsFree)
                return CellClickIntent.Move;

            if (IsFriendly(cell))
                return CellClickIntent.SelectFigure;

            return CellClickIntent.Attack;
        }

        /// <summary>
        /// Checks if cell contains a figure the current player can control.
        /// Abstracted to handle future cases: mind-control, neutral figures, etc.
        /// </summary>
        private bool IsFriendly(BoardCell cell)
        {
            return !cell.IsFree && cell.OccupiedBy?.Team == _turnSystem.CurrentTeam;
        }

        private void Select(BoardCell cell)
        {
            _selectedCell = cell;
            _logger.Debug($"Selected {cell.OccupiedBy} at ({cell.Position.Row},{cell.Position.Column})");
            _figureSelectedPublisher.Publish(new FigureSelectedMessage(cell.OccupiedBy, cell.Position));
        }

        public void ClearSelection()
        {
            if (_selectedCell != null)
                _logger.Debug("Selection cleared");

            _selectedCell = null;
            _figureSelectedPublisher.Publish(new FigureSelectedMessage(null, default));
        }

        private void RequestMove(GridPosition from, GridPosition to)
        {
            _logger.Debug($"Move: ({from.Row},{from.Column}) -> ({to.Row},{to.Column})");
            _moveRequestedPublisher.Publish(new MoveRequestedMessage(from, to));
        }

        private void RequestAttack(GridPosition from, GridPosition to)
        {
            _logger.Debug($"Attack: ({from.Row},{from.Column}) -> ({to.Row},{to.Column})");
            _attackRequestedPublisher.Publish(new AttackRequestedMessage(from, to));
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
