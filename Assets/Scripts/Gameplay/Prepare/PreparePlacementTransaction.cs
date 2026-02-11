using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Encapsulates placement flow with rollback on failure.
    /// </summary>
    public sealed class PreparePlacementTransaction
    {
        private readonly PrepareState _state;
        private readonly IPreparePresenter _presenter;
        private readonly FigureSpawnService _spawnService;
        private readonly PlayerRunStateModel _runState;
        private readonly BoardGrid _grid;
        private readonly ILogger _logger;

        public PreparePlacementTransaction(
            PrepareState state,
            IPreparePresenter presenter,
            FigureSpawnService spawnService,
            PlayerRunStateModel runState,
            BoardGrid grid,
            ILogger logger)
        {
            _state = state;
            _presenter = presenter;
            _spawnService = spawnService;
            _runState = runState;
            _grid = grid;
            _logger = logger;
        }

        public async UniTask<bool> ExecuteAsync(FigureState state, GridPosition pos, CancellationToken cancellationToken)
        {
            string figureId = state.Id;
            string figureTypeId = state.TypeId;

            _presenter.RemoveFigure(figureId);
            _state.OnPlaced(figureId);

            bool success = false;
            try
            {
                Figure figure = await _spawnService
                    .SpawnAsync(_grid, pos, figureTypeId, Team.Player)
                    .AttachExternalCancellation(cancellationToken);
                if (figure == null)
                {
                    _logger.Error("Failed to spawn figure");
                    return false;
                }

                _runState.PlaceOnBoard(figureId, pos);
                _logger.Info($"Placed {figureTypeId} at ({pos.Row}, {pos.Column})");
                success = true;
                return true;
            }
            finally
            {
                if (!success && !cancellationToken.IsCancellationRequested)
                {
                    _state.Restore(figureId);
                    try
                    {
                        await _presenter
                            .RestoreFigureAsync(figureId)
                            .AttachExternalCancellation(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to restore figure {figureId}", ex);
                    }
                }
            }
        }
    }
}
