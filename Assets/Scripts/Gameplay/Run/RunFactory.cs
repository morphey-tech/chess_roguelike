using Project.Core.Core.Configs.Run;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Phase;
using VContainer;

namespace Project.Gameplay.Gameplay.Run
{
    public class RunFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly StageFactory _stageFactory;
        private readonly StagePhaseFactory _phaseFactory;
        private readonly PlayerRunStateService _runStateService;
        private readonly BoardSpawnService _boardSpawnService;

        [Inject]
        private RunFactory(
            ConfigProvider configProvider, 
            StageFactory stageFactory,
            StagePhaseFactory phaseFactory, 
            PlayerRunStateService runStateService,
            BoardSpawnService boardSpawnService)
        {
            _configProvider = configProvider;
            _stageFactory = stageFactory;
            _phaseFactory = phaseFactory;
            _runStateService = runStateService;
            _boardSpawnService = boardSpawnService;
        }

        public Run Create(RunConfig config)
        {
            return new Run(
                config, 
                _configProvider, 
                _stageFactory, 
                _phaseFactory, 
                _runStateService,
                _boardSpawnService);
        }
    }
}
