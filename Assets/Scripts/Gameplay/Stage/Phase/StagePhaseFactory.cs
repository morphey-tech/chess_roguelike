using System.Collections.Generic;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Configs.Suites;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Selection;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public class StagePhaseFactory
    {
        private readonly BoardSpawnService _boardSpawner;
        private readonly FigureSpawnService _figureSpawner;
        private readonly MovementService _movementService;
        private readonly SelectionService _selectionService;
        private readonly ILogService _logService;

        [Inject]
        private StagePhaseFactory(
            BoardSpawnService boardSpawner,
            FigureSpawnService figureSpawner,
            MovementService movementService,
            SelectionService selectionService,
            ILogService logService)
        {
            _boardSpawner = boardSpawner;
            _figureSpawner = figureSpawner;
            _movementService = movementService;
            _selectionService = selectionService;
            _logService = logService;
        }

        public List<IStagePhase> CreatePhasesForStage(StageConfig stageConfig, SuiteConfig suiteConfig)
        {
            switch (stageConfig.Type)
            {
                case StageType.Duel:
                    return new List<IStagePhase>
                    {
                        new BoardSpawnPhase(_boardSpawner, _logService),
                        new GameplayInitPhase(_movementService, _selectionService, _logService),
                        new FigureSpawnPhase(_figureSpawner, suiteConfig, _logService)
                    };
                default:
                    return new List<IStagePhase>();
            }
        }
    }
}