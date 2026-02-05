using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Gameplay;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Prepare;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public sealed class StagePhaseFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly IFiguresSpawnProviderFactory _spawnProviderFactory;
        private readonly FigureSpawnService _figureSpawnService;
        private readonly ConfigProvider _configProvider;
        private readonly ILogService _logService;
        private readonly ILogger<StagePhaseFactory> _logger;

        private GameplayConfig _gameplayConfig;

        [Inject]
        private StagePhaseFactory(
            IObjectResolver resolver,
            IFiguresSpawnProviderFactory spawnProviderFactory,
            FigureSpawnService figureSpawnService,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _resolver = resolver;
            _spawnProviderFactory = spawnProviderFactory;
            _figureSpawnService = figureSpawnService;
            _configProvider = configProvider;
            _logService = logService;
            _logger = logService.CreateLogger<StagePhaseFactory>();
        }

        public async UniTask<List<IStagePhase>> CreatePhasesForStageAsync(StageConfig stageConfig)
        {
            // Load gameplay config if not cached
            _gameplayConfig ??= await _configProvider.Get<GameplayConfig>("gameplay_conf");
            
            List<IStagePhase> phases = new();
            Type[] pipeline = StagePipelineConfig.GetPipeline(stageConfig.Type);

            foreach (Type phaseType in pipeline)
            {
                IStagePhase phase = CreatePhase(phaseType, stageConfig);
                phases.Add(phase);
            }

            // Reorder based on config: if enemies should be visible during prepare,
            // move FiguresSpawnPhase before PreparePlacementPhase
            if (_gameplayConfig != null && !_gameplayConfig.HideEnemiesDuringPrepare)
            {
                ReorderEnemiesBeforePrepare(phases);
            }

            return phases;
        }

        private void ReorderEnemiesBeforePrepare(List<IStagePhase> phases)
        {
            int prepareIndex = phases.FindIndex(p => p is PreparePlacementPhase);
            int enemiesIndex = phases.FindIndex(p => p is FiguresSpawnPhase);

            if (prepareIndex >= 0 && enemiesIndex >= 0 && enemiesIndex > prepareIndex)
            {
                // Move enemies before prepare
                IStagePhase enemiesPhase = phases[enemiesIndex];
                phases.RemoveAt(enemiesIndex);
                phases.Insert(prepareIndex, enemiesPhase);
                _logger.Info("Reordered: enemies will spawn before prepare (HideEnemiesDuringPrepare=false)");
            }
        }

        private IStagePhase CreatePhase(Type phaseType, StageConfig stageConfig)
        {
            if (phaseType == typeof(FiguresSpawnPhase))
            {
                IFiguresSpawnProvider provider = _spawnProviderFactory.Create(stageConfig.Type);
                return new FiguresSpawnPhase(_figureSpawnService, provider, _logService);
            }

            return (IStagePhase)_resolver.Resolve(phaseType);
        }
    }
}
