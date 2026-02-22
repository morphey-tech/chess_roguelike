using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Movement;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Presentations;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class FigureSpawnService
    {
        private readonly ConfigProvider _configProvider;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IFigureStatsFactory _statsFactory;
        private readonly TurnPatternFactory _turnPatternFactory;
        private readonly BoardCapacityService _capacityService;
        private readonly MovementStrategyFactory _movementStrategyFactory;
        private readonly IPublisher<FigureSpawnedMessage> _spawnedPublisher;
        private readonly IFigureRegistry _figureRegistry;
        private readonly ILogger<FigureSpawnService> _logger;

        private FigureConfigRepository? _figureConfigCache;
        private FigureDescriptionConfigRepository? _descriptionConfigCache;
        private PassiveConfigRepository? _passiveConfigCache;

        [Inject]
        private FigureSpawnService(
            ConfigProvider configProvider,
            IFigurePresenter figurePresenter,
            IFigureStatsFactory statsFactory,
            TurnPatternFactory turnPatternFactory,
            BoardCapacityService capacityService,
            MovementStrategyFactory movementStrategyFactory,
            IPublisher<FigureSpawnedMessage> spawnedPublisher,
            IFigureRegistry figureRegistry,
            ILogService logService)
        {
            _configProvider = configProvider;
            _figurePresenter = figurePresenter;
            _statsFactory = statsFactory;
            _turnPatternFactory = turnPatternFactory;
            _capacityService = capacityService;
            _movementStrategyFactory = movementStrategyFactory;
            _spawnedPublisher = spawnedPublisher;
            _figureRegistry = figureRegistry;
            _logger = logService.CreateLogger<FigureSpawnService>();
        }

        /// <summary>
        /// Preloads all configs needed for spawning figures.
        /// Call this before gameplay to avoid delays on first spawn.
        /// </summary>
        public async UniTask PreloadConfigsAsync()
        {
            _logger.Debug("Preloading figure spawn configs...");
            
            _figureConfigCache ??= await _configProvider.Get<FigureConfigRepository>("figures_conf");
            _descriptionConfigCache ??= await _configProvider.Get<FigureDescriptionConfigRepository>("figure_descriptions_conf");
            _passiveConfigCache ??= await _configProvider.Get<PassiveConfigRepository>("passives_conf");
            
            await _turnPatternFactory.InitializeAsync();
            
            _logger.Debug("Figure spawn configs preloaded");
        }

        public async UniTask<Figure?> SpawnAsync(BoardGrid grid, GridPosition position, string figureId, Team team)
        {
            BoardCell cell = grid.GetBoardCell(position);

            if (!cell.IsFree)
            {
                _logger.Warning($"Cell ({position.Row}, {position.Column}) is already occupied");
                return null;
            }

            _figureConfigCache ??= await _configProvider.Get<FigureConfigRepository>("figures_conf");
            _descriptionConfigCache ??= await _configProvider.Get<FigureDescriptionConfigRepository>("figure_descriptions_conf");
            _passiveConfigCache ??= await _configProvider.Get<PassiveConfigRepository>("passives_conf");
            
            await _turnPatternFactory.InitializeAsync();

            FigureConfig? figureConfig = _figureConfigCache.Get(figureId);
            if (figureConfig == null)
            {
                _logger.Error($"Figure config not found: {figureId}");
                return null;
            }

            FigureDescriptionConfig? description = _descriptionConfigCache.Get(figureConfig.DescriptionId);
            if (description == null)
            {
                _logger.Error($"Description not found: {figureConfig.DescriptionId}");
                return null;
            }

            FigureStats stats = _statsFactory.Create(description.StatsId);

            // Create PatternMovement if movement_pattern is specified
            string movementId = description.MovementId;
            if (description.MovementPattern != null && description.MovementId == "pattern")
            {
                movementId = description.MovementPattern.Id;
                var patternMovement = _movementStrategyFactory.CreatePattern(description.MovementPattern);
                _movementStrategyFactory.RegisterPattern(movementId, patternMovement);
            }

            Figure figure = new(
                IdGetter.MakeId(),
                figureId,
                movementId,
                description.AttackId,
                description.TurnPatternsId,
                stats,
                team);

            if (!string.IsNullOrEmpty(description.LootTableId))
            {
                figure.LootTableId = description.LootTableId;
            }

            if (description.Passives != null)
            {
                foreach (string passiveId in description.Passives)
                {
                    PassiveConfig? passiveConfig = _passiveConfigCache.Get(passiveId);
                    if (passiveConfig != null)
                    {
                        IPassive? passive = PassiveFactory.Create(passiveConfig);
                        figure.AddPassive(passive);
                    }
                }
                
                if (figure.BasePassives.Count > 0)
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

            bool reservedCapacity = false;
            if (team == Team.Player)
            {
                reservedCapacity = _capacityService.TryReserve(description);
                if (!reservedCapacity)
                {
                    _logger.Warning($"Spawn blocked by board capacity for '{figureConfig.Id}'");
                    return null;
                }
            }

            try
            {
                grid.PlaceFigure(figure, position);
                await _figurePresenter.CreateFigure(figure, figureConfig.AssetKey, position, team);
                _figureRegistry.Register(figure);
                _logger.Info($"Spawned {figure} [{description.Id}] HP:{stats.CurrentHp}/{stats.MaxHp} ATK:{stats.Attack}");
                _spawnedPublisher.Publish(new FigureSpawnedMessage(figure, position));
                return figure;
            }
            catch
            {
                if (reservedCapacity)
                    _capacityService.ReleaseByType(figure.TypeId);
                throw;
            }
        }

        public void ClearCache()
        {
            _figureConfigCache = null;
            _descriptionConfigCache = null;
            _passiveConfigCache = null;
        }
    }
}
