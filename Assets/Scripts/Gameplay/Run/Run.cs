using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Run;
using Project.Core.Core.Configs.Stage;
using Project.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Run
{
    public class Run
    {
        private readonly RunConfig _config;
        private readonly ConfigProvider _configProvider;
        private readonly StageFactory _stageFactory;

        private int _currentStageIndex;

        public Stage.Stage CurrentStage { get; private set; }
        public bool IsCompleted => _currentStageIndex >= _config.Stages.Length;

        public Run(RunConfig config, ConfigProvider configProvider, StageFactory stageFactory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _stageFactory = stageFactory ?? throw new ArgumentNullException(nameof(stageFactory));
        }

        public void Begin()
        {
            _currentStageIndex = 0;
            LaunchCurrentStageAsync().Forget();
        }

        public void NextStage()
        {
            _currentStageIndex++;
            if (!IsCompleted)
            {
                LaunchCurrentStageAsync().Forget();
            }
        }

        private async UniTask LaunchCurrentStageAsync()
        {
            string stageId = _config.Stages[_currentStageIndex];
            
            StageConfigRepository repository = 
                await _configProvider.Get<StageConfigRepository>("stages_conf");
            
            StageConfig stageConfig = Array.Find(repository.Stages, s => s.Id == stageId);
            
            if (stageConfig == null)
            {
                throw new Exception($"Stage config '{stageId}' not found");
            }

            // Создаём грид по размерам из конфига борды (пока захардкодим)
            BoardGrid grid = new BoardGrid(8, 8);
            
            CurrentStage = _stageFactory.Create(stageConfig, grid);
            CurrentStage.Begin();
        }
    }
}
