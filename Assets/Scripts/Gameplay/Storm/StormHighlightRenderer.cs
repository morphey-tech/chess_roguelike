using System;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Core;
using Project.Core.Core.Storm.Messages;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Рендерер shrinking zone на доске
    /// </summary>
    public class StormHighlightRenderer : IDisposable
    {
        private readonly MovementService _movementService;
        private readonly ILogger<StormHighlightRenderer> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private StormHighlightRenderer(
            MovementService movementService,
            ISubscriber<StormCellsUpdatedMessage> cellsSubscriber,
            ISubscriber<StormStateChangedMessage> stateSubscriber,
            ILogService logService)
        {
            _movementService = movementService;
            _logger = logService.CreateLogger<StormHighlightRenderer>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            cellsSubscriber.Subscribe(OnZoneCellsUpdated).AddTo(bag);
            stateSubscriber.Subscribe(OnZoneStateChanged).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnZoneCellsUpdated(StormCellsUpdatedMessage msg)
        {
            if (_movementService.Grid == null)
            {
                _logger.Warning("Grid is null");
                return;
            }

            ClearAllCells();

            foreach (GridPosition pos in msg.WarningCells)
            {
                ApplyWarning(pos);
            }
            foreach (GridPosition pos in msg.DangerCells)
            {
                ApplyDanger(pos);
            }

            _logger.Debug($"Zone rendered: {msg.WarningCells.Length} warning, {msg.DangerCells.Length} danger");
        }

        private void OnZoneStateChanged(StormStateChangedMessage msg)
        {
            if (msg.NewState == StormState.Inactive)
            {
                ClearAllCells();
            }
        }

        private void ApplyWarning(GridPosition pos)
        {
            BoardCell boardCell = _movementService.Grid.GetBoardCell(pos);
            boardCell.Del<StormDangerTag>();
            boardCell.EnsureComponent(new StormWarningTag());
        }

        private void ApplyDanger(GridPosition pos)
        {
            BoardCell boardCell = _movementService.Grid.GetBoardCell(pos);
            boardCell.Del<StormWarningTag>();
            boardCell.EnsureComponent(new StormDangerTag());
        }

        private void ClearAllCells()
        {
            if (_movementService.Grid == null)
            {
                return;
            }
            foreach (BoardCell? cell in _movementService.Grid.AllCells())
            {
                cell.Del<StormWarningTag>();
                cell.Del<StormDangerTag>();
            }
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
            ClearAllCells();
        }
    }
}
