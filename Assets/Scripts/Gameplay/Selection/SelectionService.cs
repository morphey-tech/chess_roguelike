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
        private readonly ILogger<SelectionService> _logger;
        private readonly IDisposable _subscriptions;

        private BoardGrid _grid;
        private BoardCell? _selectedCell;

        public bool HasSelection => _selectedCell != null;
        public Figure? SelectedFigure => _selectedCell?.OccupiedBy;

        [Inject]
        private SelectionService(
            TurnSystem turnSystem,
            ISubscriber<CellClickedMessage> cellClickedSubscriber,
            ISubscriber<CancelRequestedMessage> cancelSubscriber,
            IPublisher<FigureSelectedMessage> figureSelectedPublisher,
            IPublisher<MoveRequestedMessage> moveRequestedPublisher,
            ILogService logService)
        {
            _turnSystem = turnSystem;
            _figureSelectedPublisher = figureSelectedPublisher;
            _moveRequestedPublisher = moveRequestedPublisher;
            _logger = logService.CreateLogger<SelectionService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            cellClickedSubscriber.Subscribe(OnCellClicked).AddTo(bag);
            cancelSubscriber.Subscribe(_ => ClearSelection()).AddTo(bag);
            _subscriptions = bag.Build();
            
            _logger.Info("SelectionService created with input subscriptions");
        }

        public void Configure(BoardGrid grid)
        {
            _grid = grid;
            ClearSelection();
        }

        private void OnCellClicked(CellClickedMessage message)
        {
            GridPosition position = message.Position;
            _logger.Info($"Cell clicked: ({position.Row}, {position.Column})");
            
            if (_grid == null)
            {
                _logger.Warning("Grid not set!");
                return;
            }

            if (!_turnSystem.IsPlayerTurn)
            {
                _logger.Debug("Not player's turn, ignoring");
                return;
            }

            if (!_grid.IsInside(position))
            {
                _logger.Debug($"Position ({position.Row}, {position.Column}) is outside grid {_grid.Width}x{_grid.Height}");
                return;
            }

            BoardCell clickedCell = _grid.GetBoardCell(position);
            bool hasFigure = clickedCell.OccupiedBy != null;
            _logger.Debug($"Cell has figure: {hasFigure}, IsFree: {clickedCell.IsFree}");

            if (HasSelection)
            {
                if (clickedCell.IsFree)
                {
                    TryMove(_selectedCell.Position, position);
                    ClearSelection();
                }
                else if (clickedCell.OccupiedBy?.Team == _turnSystem.CurrentTeam)
                {
                    Select(clickedCell);
                }
                else
                {
                    ClearSelection();
                }
            }
            else
            {
                if (!clickedCell.IsFree && clickedCell.OccupiedBy?.Team == _turnSystem.CurrentTeam)
                {
                    Select(clickedCell);
                }
                else
                {
                    _logger.Debug($"Cannot select: IsFree={clickedCell.IsFree}, Team={clickedCell.OccupiedBy?.Team}, CurrentTeam={_turnSystem.CurrentTeam}");
                }
            }
        }

        private void Select(BoardCell cell)
        {
            _selectedCell = cell;
            _logger.Debug($"Selected figure at ({cell.Position.Row},{cell.Position.Column})");
            _figureSelectedPublisher.Publish(new FigureSelectedMessage(cell.OccupiedBy, cell.Position));
        }

        public void ClearSelection()
        {
            if (_selectedCell != null)
            {
                _logger.Debug("Selection cleared");
            }
            _selectedCell = null;
            _figureSelectedPublisher.Publish(new FigureSelectedMessage(null, default));
        }

        private void TryMove(GridPosition from, GridPosition to)
        {
            _logger.Debug($"Requesting move from ({from.Row},{from.Column}) to ({to.Row},{to.Column})");
            _moveRequestedPublisher.Publish(new MoveRequestedMessage(from, to));
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
