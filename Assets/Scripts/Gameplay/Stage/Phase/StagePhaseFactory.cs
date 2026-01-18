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
        public StagePhaseFactory(
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
            List<IStagePhase> phases = new()
            {
                _resolver.Resolve<BoardSpawnPhase>()
            };

            IFiguresSpawnProvider provider = _spawnProviderFactory.Create(stageConfig.Type);
            phases.Add(new FiguresSpawnPhase(_figureSpawnService, provider, _logService));

            if (stageConfig.Type == StageType.Duel)
            {
                phases.Add(_resolver.Resolve<PreparePlacementPhase>());
            }

            phases.Add(_resolver.Resolve<GameplayInitPhase>());

            return phases;
        }
    }
}
