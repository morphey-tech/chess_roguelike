using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Boards;
using Project.Core.Core.Configs.Run;
using Project.Core.Core.Configs.Stage;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Phase;

namespace Project.Gameplay.Gameplay.Run
{
    public class Run
    {
        public Stage.Stage? CurrentStage { get; private set; }
        public bool IsCompleted => GetCurrentStageIndex() >= _config.Stages.Length;

        private readonly RunConfig _config;
        private readonly ConfigProvider _configProvider;
        private readonly StageFactory _stageFactory;
        private readonly StagePhaseFactory _phaseFactory;
        private readonly PlayerRunStateService _runStateService;
        private readonly BoardSpawnService _boardSpawnService;
        private readonly BoardCapacityService _boardCapacityService;

        public Run(
            RunConfig config,
            ConfigProvider configProvider,
            StageFactory stageFactory,
            StagePhaseFactory phaseFactory,
            PlayerRunStateService runStateService,
            BoardSpawnService boardSpawnService,
            BoardCapacityService boardCapacityService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _stageFactory = stageFactory ?? throw new ArgumentNullException(nameof(stageFactory));
            _phaseFactory = phaseFactory ?? throw new ArgumentNullException(nameof(phaseFactory));
            _runStateService = runStateService ?? throw new ArgumentNullException(nameof(runStateService));
            _boardSpawnService = boardSpawnService ?? throw new ArgumentNullException(nameof(boardSpawnService));
            _boardCapacityService = boardCapacityService ?? throw new ArgumentNullException(nameof(boardCapacityService));
        }

        public async UniTask Begin()
        {
            await LaunchCurrentStageAsync();
        }

        public async UniTask RestartCurrentStageAsync()
        {
            CancelCurrentStage();
            await LaunchCurrentStageAsync();
        }

        public bool CanAdvanceToNextStage()
        {
            int nextIndex = _runStateService.Current?.CurrentStageIndex + 1 ?? GetCurrentStageIndex() + 1;
            return nextIndex < _config.Stages.Length;
        }

        public async UniTask<bool> NextStageAsync()
        {
            int nextIndex = _runStateService.Current?.CurrentStageIndex + 1 ?? GetCurrentStageIndex() + 1;
            if (nextIndex >= _config.Stages.Length)
            {
                return false;
            }

            _runStateService.Current!.CurrentStageIndex = nextIndex;
            _runStateService.Current.StageId = _config.Stages[nextIndex];
            CancelCurrentStage();
            await LaunchCurrentStageAsync();
            return true;
        }

        private async UniTask LaunchCurrentStageAsync()
        {
            StageConfig stageConfig = await LoadStageConfig();
            BoardConfig boardConfig = await LoadBoardConfig(stageConfig);
            await _boardCapacityService.InitializeForBoardAsync(boardConfig);
            _boardCapacityService.Reset();

            BoardGrid grid = await _boardSpawnService.GetGridAsync(boardConfig.Id);
            List<IStagePhase> phases = await _phaseFactory.CreatePhasesForStageAsync(stageConfig);
            CurrentStage = _stageFactory.Create(stageConfig, grid, _runStateService.Current!, phases);
            await CurrentStage.BeginAsync();
        }

        private void CancelCurrentStage()
        {
            if (CurrentStage == null)
            {
                return;
            }

            CurrentStage.Abort();
            CurrentStage.Dispose();
            CurrentStage = null;
        }

        private int GetCurrentStageIndex()
        {
            if (_runStateService.Current != null)
            {
                int idx = _runStateService.Current.CurrentStageIndex;
                if (idx >= 0 && idx < _config.Stages.Length)
                {
                    return idx;
                }

                string currentStageId = _runStateService.Current.StageId ?? _config.Stages[0];
                idx = Array.IndexOf(_config.Stages, currentStageId);
                if (idx >= 0)
                {
                    _runStateService.Current.CurrentStageIndex = idx;
                    return idx;
                }
            }
            return 0;
        }

        private async UniTask<StageConfig> LoadStageConfig()
        {
            int stageIndex = GetCurrentStageIndex();
            string stageId = _config.Stages[stageIndex];
            if (_runStateService.Current != null)
            {
                _runStateService.Current.StageId = stageId;
            }

            StageConfigRepository stageRepository = 
                await _configProvider.Get<StageConfigRepository>("stages_conf");
            StageConfig? stageConfig = stageRepository.Get(stageId);
            return stageConfig ?? throw new NullReferenceException($"Stage config '{stageId}' not found");
        }

        private async UniTask<BoardConfig> LoadBoardConfig(StageConfig stageConfig)
        {
            BoardConfigRepository boardRepository = await _configProvider.Get<BoardConfigRepository>("boards_conf");
            BoardConfig? boardConfig = boardRepository.Get(stageConfig.BoardId);
            return boardConfig ?? throw new NullReferenceException($"Board config '{stageConfig.BoardId}' not found");
        }
    }
}
