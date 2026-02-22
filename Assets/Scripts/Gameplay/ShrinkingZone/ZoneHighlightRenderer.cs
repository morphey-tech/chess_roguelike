using System;
using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.ShrinkingZone.Core;
using Project.Core.Core.ShrinkingZone.Messages;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Компонент-тег для подсветки warning зоны
    /// </summary>
    public class ZoneWarningTag : IEntityComponent
    {
    }

    /// <summary>
    /// Компонент-тег для подсветки danger зоны
    /// </summary>
    public class ZoneDangerTag : IEntityComponent
    {
    }

    /// <summary>
    /// Рендерер shrinking zone на доске
    /// </summary>
    public class ZoneHighlightRenderer : IDisposable
    {
        private readonly MovementService _movementService;
        private readonly ILogger<ZoneHighlightRenderer> _logger;
        private readonly IDisposable _subscriptions;

        [Inject]
        private ZoneHighlightRenderer(
            MovementService movementService,
            ISubscriber<ZoneCellsUpdatedMessage> cellsSubscriber,
            ISubscriber<ZoneStateChangedMessage> stateSubscriber,
            ILogService logService)
        {
            _movementService = movementService;
            _logger = logService.CreateLogger<ZoneHighlightRenderer>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            cellsSubscriber.Subscribe(OnZoneCellsUpdated).AddTo(bag);
            stateSubscriber.Subscribe(OnZoneStateChanged).AddTo(bag);
            _subscriptions = bag.Build();
        }

        private void OnZoneCellsUpdated(ZoneCellsUpdatedMessage msg)
        {
            if (_movementService.Grid == null)
            {
                _logger.Warning("ZoneHighlightRenderer: Grid is null");
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

        private void OnZoneStateChanged(ZoneStateChangedMessage msg)
        {
            if (msg.NewState == ZoneState.Inactive)
            {
                ClearAllCells();
            }
        }

        private void ApplyWarning(GridPosition pos)
        {
            BoardCell boardCell = _movementService.Grid.GetBoardCell(pos);
            boardCell.Del<ZoneDangerTag>();
            boardCell.EnsureComponent(new ZoneWarningTag());
        }

        private void ApplyDanger(GridPosition pos)
        {
            BoardCell boardCell = _movementService.Grid.GetBoardCell(pos);
            boardCell.Del<ZoneWarningTag>();
            boardCell.EnsureComponent(new ZoneDangerTag());
        }

        private void ClearAllCells()
        {
            if (_movementService.Grid == null)
            {
                return;
            }
            foreach (BoardCell? cell in _movementService.Grid.AllCells())
            {
                cell.Del<ZoneWarningTag>();
                cell.Del<ZoneDangerTag>();
            }
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
            ClearAllCells();
        }
    }
}
