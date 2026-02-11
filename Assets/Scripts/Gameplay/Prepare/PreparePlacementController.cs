using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Handles placement transactions and updates placement cache state.
    /// </summary>
    public sealed class PreparePlacementController
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly IPreparePresenter _preparePresenter;
        private readonly ILogger<PreparePlacementController> _logger;
        private bool _isPlacing;

        public PreparePlacementController(
            FigureSpawnService figureSpawnService,
            IPreparePresenter preparePresenter,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _preparePresenter = preparePresenter;
            _logger = logService.CreateLogger<PreparePlacementController>();
        }

        public async UniTask<PreparePlacementResult> PlaceSelectedAsync(
            PrepareContext context,
            GridPosition pos,
            CancellationToken cancellationToken)
        {
            if (_isPlacing)
                return PreparePlacementResult.Ignored;

            _isPlacing = true;
            try
            {
                if (!context.State.IsActive)
                    return PreparePlacementResult.Ignored;

                var state = context.GetSelectedFigure();
                if (state == null)
                    return PreparePlacementResult.Ignored;

                context.PreviousSelectedId = null;

                var transaction = new PreparePlacementTransaction(
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
                    context.AvailablePlacementPositions.Add(pos);

                return new PreparePlacementResult(true, false, false);
            }
            finally
            {
                _isPlacing = false;
            }
        }
    }

    public readonly struct PreparePlacementResult
    {
        public static PreparePlacementResult Ignored { get; } = new(false, false, false);

        public bool Processed { get; }
        public bool Success { get; }
        public bool Completed { get; }

        public PreparePlacementResult(bool processed, bool success, bool completed)
        {
            Processed = processed;
            Success = success;
            Completed = completed;
        }
    }
}
