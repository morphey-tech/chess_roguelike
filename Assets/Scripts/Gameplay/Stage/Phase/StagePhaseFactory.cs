using System;
using System.Collections.Generic;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public sealed class StagePhaseFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly IFiguresSpawnProviderFactory _spawnProviderFactory;
        private readonly FigureSpawnService _figureSpawnService;
        private readonly ILogService _logService;

        [Inject]
        private StagePhaseFactory(
            IObjectResolver resolver,
            IFiguresSpawnProviderFactory spawnProviderFactory,
            FigureSpawnService figureSpawnService,
            ILogService logService)
        {
            _resolver = resolver;
            _spawnProviderFactory = spawnProviderFactory;
            _figureSpawnService = figureSpawnService;
            _logService = logService;
        }

        public List<IStagePhase> CreatePhasesForStage(StageConfig stageConfig)
        {
            List<IStagePhase> phases = new();
            Type[] pipeline = StagePipelineConfig.GetPipeline(stageConfig.Type);

            foreach (Type phaseType in pipeline)
            {
                IStagePhase phase = CreatePhase(phaseType, stageConfig);
                phases.Add(phase);
            }

            return phases;
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
