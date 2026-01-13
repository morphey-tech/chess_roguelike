using Project.Core.Core.Configs.Run;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Run
{
    public class RunFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly StageFactory _stageFactory;

        public RunFactory(ConfigProvider configProvider, StageFactory stageFactory)
        {
            _configProvider = configProvider;
            _stageFactory = stageFactory;
        }

        public Run Create(RunConfig config)
        {
            return new Run(config, _configProvider, _stageFactory);
        }
    }
}
