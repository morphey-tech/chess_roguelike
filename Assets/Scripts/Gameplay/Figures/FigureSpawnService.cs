using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Presentations;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class FigureSpawnService
    {
        private readonly ConfigProvider _configProvider;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureSpawnedMessage> _spawnedPublisher;
        private readonly ILogger<FigureSpawnService> _logger;
        
        private FigureConfigRepository? _figureConfigCache;
        private StatsConfigRepository? _statsConfigCache;
        private PassiveConfigRepository? _passiveConfigCache;

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

        public async UniTask<Figure?> SpawnAsync(BoardGrid grid, GridPosition position, string figureTypeId, Team team)
        {
            BoardCell cell = grid.GetBoardCell(position);

            if (!cell.IsFree)
            {
                _logger.Warning($"Cell ({position.Row}, {position.Column}) is already occupied");
                return null;
            }

            _figureConfigCache ??= await _configProvider.Get<FigureConfigRepository>("figures_conf");
            _statsConfigCache ??= await _configProvider.Get<StatsConfigRepository>("stats_conf");
            _passiveConfigCache ??= await _configProvider.Get<PassiveConfigRepository>("passives_conf");
            
            FigureConfig figureConfig = Array.Find(_figureConfigCache.Figures, f => f.Id == figureTypeId);
            string movementId = figureConfig?.MovementId ?? "pawn";
            string attackId = figureConfig?.AttackId ?? "simple";
            
            string statsId = figureConfig?.StatsId ?? figureTypeId;
            StatsConfig statsConfig = Array.Find(_statsConfigCache.Configs, s => s.Id == statsId);
            
            FigureStats stats = statsConfig != null 
                ? new FigureStats(statsConfig.MaxHp, statsConfig.Attack, statsConfig.AttackRange)
                : new FigureStats(1, 1, 1);

            Figure figure = new(IdGetter.MakeId(), figureTypeId, movementId, attackId, stats, team);
            
            if (figureConfig?.Passives != null)
            {
                foreach (string passiveId in figureConfig.Passives)
                {
                    PassiveConfig passiveConfig = Array.Find(_passiveConfigCache.Passives, p => p.Id == passiveId);
                    if (passiveConfig != null)
                    {
                        IPassive? passive = PassiveFactory.Create(passiveConfig);
                        figure.AddPassive(passive);
                    }
                }
                
                if (figure.Passives.Count > 0)
                    _logger.Debug($"{figure} passives: {string.Join(", ", figureConfig.Passives)}");
            }
            
            cell.PlaceFigure(figure);
            await _figurePresenter.CreateFigure(figure.Id, figure.TypeId, position, team);
            
            _logger.Info($"Spawned {figure} HP:{stats.CurrentHp}/{stats.MaxHp} ATK:{stats.Attack} RNG:{stats.AttackRange}");
            _spawnedPublisher.Publish(new FigureSpawnedMessage(figure, position));

            return figure;
        }
    }
}
