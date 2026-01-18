using System.Collections.Generic;
using Project.Core.Core.Configs.Stage;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public class StagePhaseFactory
    {
        private readonly IObjectResolver _resolver;

        [Inject]
        private StagePhaseFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public List<IStagePhase> CreatePhasesForStage(StageConfig stageConfig)
        {
            return stageConfig.Type switch
            {
                StageType.Duel => new List<IStagePhase>
                {
                    _resolver.Resolve<BoardSpawnPhase>(),
                    _resolver.Resolve<EnemySpawnPhase>(),
                    _resolver.Resolve<PreparePlacementPhase>(),
                    _resolver.Resolve<GameplayInitPhase>()
                },
                _ => new List<IStagePhase>()
            };
        }
    }
}
