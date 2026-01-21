using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Presentations;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Spawns figures on the board.
    /// Loads config to get movementId and stats.
    /// </summary>
    public sealed class FigureSpawnService
    {
        private readonly ConfigProvider _configProvider;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureSpawnedMessage> _spawnedPublisher;
        private readonly ILogger<FigureSpawnService> _logger;
        
        private FigureConfigRepository _figureConfigCache;
        private StatsConfigRepository _statsConfigCache;

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

            // Load configs
            _figureConfigCache ??= await _configProvider.Get<FigureConfigRepository>("figures_conf");
            _statsConfigCache ??= await _configProvider.Get<StatsConfigRepository>("stats_conf");
            
            FigureConfig figureConfig = Array.Find(_figureConfigCache.Figures, f => f.Id == figureTypeId);
            string movementId = figureConfig?.MovementId ?? "pawn";
            
            // Load stats
            string statsId = figureConfig?.StatsId ?? figureTypeId; // Default to figure id
            StatsConfig statsConfig = Array.Find(_statsConfigCache.Configs, s => s.Id == statsId);
            
            FigureStats stats = statsConfig != null 
                ? new FigureStats(statsConfig.MaxHp, statsConfig.Attack)
                : new FigureStats(1, 1); // Default stats

            Figure figure = new(IdGetter.MakeId(), figureTypeId, movementId, stats, team);
            cell.PlaceFigure(figure);

            await _figurePresenter.CreateFigure(figure.Id, figure.TypeId, position, team);
            
            _logger.Info($"Spawned {figure} HP:{stats.CurrentHp}/{stats.MaxHp} ATK:{stats.Attack} at ({position.Row}, {position.Column})");
            _spawnedPublisher.Publish(new FigureSpawnedMessage(figure, position));

            return figure;
        }
    }
}
