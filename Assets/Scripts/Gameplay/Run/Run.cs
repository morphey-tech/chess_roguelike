using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Boards;
using Project.Core.Core.Configs.Run;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Configs.Suites;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Phase;

namespace Project.Gameplay.Gameplay.Run
{
    public class Run
    {
        public Stage.Stage CurrentStage { get; private set; }
        public bool IsCompleted => _currentStageIndex >= _config.Stages.Length;
        
        private readonly RunConfig _config;
        private readonly ConfigProvider _configProvider;
        private readonly StageFactory _stageFactory;
        private readonly StagePhaseFactory _phaseFactory;
        private readonly PlayerLoadoutModel _loadoutModel;

        private int _currentStageIndex;

        public Run(RunConfig config, ConfigProvider configProvider,
            StageFactory stageFactory, StagePhaseFactory phaseFactory, PlayerLoadoutModel loadoutModel)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _stageFactory = stageFactory ?? throw new ArgumentNullException(nameof(stageFactory));
            _phaseFactory = phaseFactory ?? throw new ArgumentNullException(nameof(phaseFactory));
            _loadoutModel = loadoutModel ?? throw new ArgumentNullException(nameof(loadoutModel));
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
            StageConfig stageConfig = await LoadStageConfig();
            BoardConfig boardConfig = await LoadBoardConfig(stageConfig);
            SuiteConfig suiteConfig = await LoadSuiteConfig();
            
            BoardGrid grid = new(boardConfig.Width, boardConfig.Height);
            List<IStagePhase> phases = _phaseFactory.CreatePhasesForStage(stageConfig, suiteConfig);
            CurrentStage = _stageFactory.Create(stageConfig, grid, phases);
            await CurrentStage.BeginAsync();
        }

        private async UniTask<StageConfig> LoadStageConfig()
        {
            string stageId = _config.Stages[_currentStageIndex];
            StageConfigRepository stageRepository = 
                await _configProvider.Get<StageConfigRepository>("stages_conf");
            StageConfig stageConfig = Array.Find(stageRepository.Stages, s => s.Id == stageId);
            return stageConfig ?? throw new NullReferenceException($"Stage config '{stageId}' not found");
        }

        private async UniTask<BoardConfig> LoadBoardConfig(StageConfig stageConfig)
        {
            BoardConfigRepository boardRepository = await _configProvider.Get<BoardConfigRepository>("boards_conf");
            BoardConfig? boardConfig = boardRepository.GetBy(stageConfig.BoardId);
            return boardConfig ?? throw new NullReferenceException($"Board config '{stageConfig.BoardId}' not found");
        }

        private async UniTask<SuiteConfig> LoadSuiteConfig()
        {
            SuiteConfigRepository suiteRepository = 
                await _configProvider.Get<SuiteConfigRepository>("suites_conf");
            SuiteConfig suiteConfig = Array.Find(suiteRepository.Suites, s => s.Id == _loadoutModel.SuiteId);
            return suiteConfig ?? throw new NullReferenceException($"Suite config '{_loadoutModel.SuiteId}' not found");
        }
    }
}
