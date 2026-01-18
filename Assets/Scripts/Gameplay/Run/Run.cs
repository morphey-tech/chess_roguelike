using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Boards;
using Project.Core.Core.Configs.Run;
using Project.Core.Core.Configs.Stage;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Phase;

namespace Project.Gameplay.Gameplay.Run
{
    public class Run
    {
        public Stage.Stage CurrentStage { get; private set; }
        public bool IsCompleted => GetCurrentStageIndex() >= _config.Stages.Length;
        
        private readonly RunConfig _config;
        private readonly ConfigProvider _configProvider;
        private readonly StageFactory _stageFactory;
        private readonly StagePhaseFactory _phaseFactory;
        private readonly PlayerRunStateService _runStateService;

        public Run(
            RunConfig config, 
            ConfigProvider configProvider,
            StageFactory stageFactory, 
            StagePhaseFactory phaseFactory, 
            PlayerRunStateService runStateService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _stageFactory = stageFactory ?? throw new ArgumentNullException(nameof(stageFactory));
            _phaseFactory = phaseFactory ?? throw new ArgumentNullException(nameof(phaseFactory));
            _runStateService = runStateService ?? throw new ArgumentNullException(nameof(runStateService));
        }

        public void Begin()
        {
            LaunchCurrentStageAsync().Forget();
        }

        public void NextStage()
        {
            int nextIndex = GetCurrentStageIndex() + 1;
            if (nextIndex < _config.Stages.Length)
            {
                _runStateService.Current!.StageId = _config.Stages[nextIndex];
                LaunchCurrentStageAsync().Forget();
            }
        }

        private async UniTask LaunchCurrentStageAsync()
        {
            StageConfig stageConfig = await LoadStageConfig();
            BoardConfig boardConfig = await LoadBoardConfig(stageConfig);
            
            BoardGrid grid = new(boardConfig.Width, boardConfig.Height);
            List<IStagePhase> phases = _phaseFactory.CreatePhasesForStage(stageConfig);
            CurrentStage = _stageFactory.Create(stageConfig, grid, _runStateService.Current!, phases);
            await CurrentStage.BeginAsync();
        }

        private int GetCurrentStageIndex()
        {
            string currentStageId = _runStateService.Current?.StageId ?? _config.Stages[0];
            return Array.IndexOf(_config.Stages, currentStageId);
        }

        private async UniTask<StageConfig> LoadStageConfig()
        {
            string stageId = _runStateService.Current?.StageId ?? _config.Stages[0];
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
    }
}
