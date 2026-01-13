using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Spawns figures on the board.
    /// Loads config to get movementId.
    /// </summary>
    public sealed class FigureSpawnService
    {
        private readonly ConfigProvider _configProvider;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureSpawnedMessage> _spawnedPublisher;
        private readonly ILogger<FigureSpawnService> _logger;
        
        private FigureConfigRepository _configCache;

        [Inject]
        private FigureSpawnService(
            ConfigProvider configProvider,
            IFigurePresenter figurePresenter,
            IPublisher<FigureSpawnedMessage> spawnedPublisher,
            ILogService logService)
        {
            _configProvider = configProvider;
            _figurePresenter = figurePresenter;
            _spawnedPublisher = spawnedPublisher;
            _logger = logService.CreateLogger<FigureSpawnService>();
        }

        public async UniTask<Figure> SpawnAsync(BoardGrid grid, GridPosition position, string figureTypeId, Team team)
        {
            BoardCell cell = grid.GetBoardCell(position);

            if (!cell.IsFree)
            {
                _logger.Warning($"Cell ({position.Row}, {position.Column}) is already occupied");
                return null;
            }

            // Load config to get movementId
            _configCache ??= await _configProvider.Get<FigureConfigRepository>("figures_conf");
            FigureConfig config = System.Array.Find(_configCache.Figures, f => f.Id == figureTypeId);
            
            string movementId = config?.MovementId ?? "pawn"; // Default to pawn

            Figure figure = new(figureTypeId, movementId, new FigureStats(), team);
            cell.PlaceFigure(figure);

            await _figurePresenter.CreateFigure(figure.Id, figure.TypeId, position, team);
            
            _logger.Info($"Spawned {figure} (movement: {movementId}) at ({position.Row}, {position.Column})");
            _spawnedPublisher.Publish(new FigureSpawnedMessage(figure, position));

            return figure;
        }
    }
}
