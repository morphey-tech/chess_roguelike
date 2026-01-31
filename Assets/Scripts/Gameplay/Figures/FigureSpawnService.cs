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
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Presentations;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class FigureSpawnService
    {
        private readonly ConfigProvider _configProvider;
        private readonly IFigurePresenter _figurePresenter;
        private readonly TurnPatternFactory _turnPatternFactory;
        private readonly IPublisher<FigureSpawnedMessage> _spawnedPublisher;
        private readonly ILogger<FigureSpawnService> _logger;
        
        private FigureConfigRepository _figureConfigCache;
        private FigureDescriptionConfigRepository _descriptionConfigCache;
        private StatsConfigRepository _statsConfigCache;
        private PassiveConfigRepository _passiveConfigCache;

        [Inject]
        private FigureSpawnService(
            ConfigProvider configProvider,
            IFigurePresenter figurePresenter,
            TurnPatternFactory turnPatternFactory,
            IPublisher<FigureSpawnedMessage> spawnedPublisher,
            ILogService logService)
        {
            _configProvider = configProvider;
            _figurePresenter = figurePresenter;
            _turnPatternFactory = turnPatternFactory;
            _spawnedPublisher = spawnedPublisher;
            _logger = logService.CreateLogger<FigureSpawnService>();
        }

        public async UniTask<Figure> SpawnAsync(BoardGrid grid, GridPosition position, string figureId, Team team)
        {
            BoardCell cell = grid.GetBoardCell(position);

            if (!cell.IsFree)
            {
                _logger.Warning($"Cell ({position.Row}, {position.Column}) is already occupied");
                return null;
            }

            _figureConfigCache ??= await _configProvider.Get<FigureConfigRepository>("figures_conf");
            _descriptionConfigCache ??= await _configProvider.Get<FigureDescriptionConfigRepository>("figure_descriptions_conf");
            _statsConfigCache ??= await _configProvider.Get<StatsConfigRepository>("stats_conf");
            _passiveConfigCache ??= await _configProvider.Get<PassiveConfigRepository>("passives_conf");
            
            await _turnPatternFactory.InitializeAsync();

            FigureConfig figureConfig = Array.Find(_figureConfigCache.Figures, f => f.Id == figureId);
            if (figureConfig == null)
            {
                _logger.Error($"Figure config not found: {figureId}");
                return null;
            }

            FigureDescriptionConfig description = Array.Find(
                _descriptionConfigCache.Descriptions, 
                d => d.Id == figureConfig.DescriptionId);
            
            if (description == null)
            {
                _logger.Error($"Description not found: {figureConfig.DescriptionId}");
                return null;
            }

            StatsConfig statsConfig = Array.Find(_statsConfigCache.Configs, s => s.Id == description.StatsId);
            FigureStats stats = statsConfig != null 
                ? new FigureStats(statsConfig.MaxHp, statsConfig.Attack, statsConfig.AttackRange)
                : new FigureStats(1, 1);

            Figure figure = new(
                IdGetter.MakeId(), 
                figureId,
                description.MovementId, 
                description.AttackId, 
                description.TurnPatternsId,
                stats, 
                team);

            if (description.Passives != null)
            {
                foreach (string passiveId in description.Passives)
                {
                    PassiveConfig passiveConfig = Array.Find(_passiveConfigCache.Passives, p => p.Id == passiveId);
                    if (passiveConfig != null)
                    {
                        IPassive passive = PassiveFactory.Create(passiveConfig);
                        figure.AddPassive(passive);
                    }
                }
                
                if (figure.Passives.Count > 0)
                    _logger.Debug($"{figure} passives: {string.Join(", ", description.Passives)}");
            }

            try
            {
                TurnPattern pattern = _turnPatternFactory.CreatePatternSet(description.TurnPatternsId);
                figure.SetTurnPatternSet(pattern);
            }
            catch (Exception e)
            {
                _logger.Warning($"Failed to create turn patterns '{description.TurnPatternsId}': {e.Message}");
            }

            cell.PlaceFigure(figure);
            await _figurePresenter.CreateFigure(figure, figureConfig.AssetKey, position, team);
            
            _logger.Info($"Spawned {figure} [{description.Id}] HP:{stats.CurrentHp}/{stats.MaxHp} ATK:{stats.Attack}");
            _spawnedPublisher.Publish(new FigureSpawnedMessage(figure, position));

            return figure;
        }
    }
}
