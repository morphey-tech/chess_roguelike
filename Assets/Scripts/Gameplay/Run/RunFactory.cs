using Project.Core.Core.Configs.Run;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Stage;
using VContainer;

namespace Project.Gameplay.Gameplay.Run
{
    public class RunFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly StageFactory _stageFactory;
        private readonly PlayerLoadoutService _loadoutService;

        [Inject]
        private RunFactory(ConfigProvider configProvider, StageFactory stageFactory,
            PlayerLoadoutService loadoutService)
        {
            _configProvider = configProvider;
            _stageFactory = stageFactory;
            _loadoutService = loadoutService;
        }

        public Run Create(RunConfig config)
        {
            return new Run(config, _configProvider, _stageFactory, _loadoutService.Current);
        }
    }
}
