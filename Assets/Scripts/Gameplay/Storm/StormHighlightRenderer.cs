using System;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Core;
using Project.Core.Core.Storm.Messages;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Рендерер shrinking zone на доске
    /// </summary>
    public class StormHighlightRenderer : IInitializable, IDisposable
    {
        private readonly MovementService _movementService;
        private readonly ISubscriber<string, StormMessage> _stormSubscriber;
        private readonly ILogger<StormHighlightRenderer> _logger;
        
        private IDisposable _disposable = null!;

        [Inject]
        private StormHighlightRenderer(
            MovementService movementService,
            ISubscriber<string, StormMessage> stormSubscriber,
            ILogService logService)
        {
            _movementService = movementService;
            _stormSubscriber = stormSubscriber;
            _logger = logService.CreateLogger<StormHighlightRenderer>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _stormSubscriber.Subscribe(StormMessage.CELLS_UPDATED, OnZoneCellsUpdated).AddTo(bag);
            _stormSubscriber.Subscribe(StormMessage.STATE_CHANGED, OnZoneStateChanged).AddTo(bag);
            _disposable = bag.Build();
        }

        private void OnZoneCellsUpdated(StormMessage msg)
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

        private void OnZoneStateChanged(StormMessage msg)
        {
            if (msg.State == StormState.Inactive)
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
            _disposable?.Dispose();
            ClearAllCells();
        }
    }
}
