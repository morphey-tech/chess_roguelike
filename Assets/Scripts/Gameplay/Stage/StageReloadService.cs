using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Save.Models;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageReloadService
    {
        private readonly ConfigHotReloadService _configHotReload;
        private readonly PlayerRunStateService _runStateService;
        private readonly RunHolder _runHolder;
        private readonly PrepareService _prepareService;
        private readonly InteractionController _interactionController;
        private readonly InteractionLockService _interactionLock;
        private readonly GameShutdownCleanupService _cleanupService;
        private readonly FigureSpawnService _figureSpawnService;
        private readonly FigureStatsFactory _figureStatsFactory;
        private readonly TurnPatternFactory _turnPatternFactory;
        private readonly ILogger _logger;
        private int _reloading;

        public StageReloadService(
            ConfigHotReloadService configHotReload,
            PlayerRunStateService runStateService,
            RunHolder runHolder,
            PrepareService prepareService,
            InteractionController interactionController,
            InteractionLockService interactionLock,
            GameShutdownCleanupService cleanupService,
            FigureSpawnService figureSpawnService,
            FigureStatsFactory figureStatsFactory,
            TurnPatternFactory turnPatternFactory,
            ILogService logService)
        {
            _configHotReload = configHotReload;
            _runStateService = runStateService;
            _runHolder = runHolder;
            _prepareService = prepareService;
            _interactionController = interactionController;
            _interactionLock = interactionLock;
            _cleanupService = cleanupService;
            _figureSpawnService = figureSpawnService;
            _figureStatsFactory = figureStatsFactory;
            _turnPatternFactory = turnPatternFactory;
            _logger = logService.CreateLogger<StageReloadService>();
        }

        public async UniTask ReloadCurrentStageAsync()
        {
            if (Interlocked.Exchange(ref _reloading, 1) == 1)
            {
                _logger.Warning("Stage reload already in progress");
                return;
            }

            try
            {
                if (!_runStateService.HasRun || _runStateService.Current == null || _runHolder.Current == null)
                {
                    _logger.Warning("Cannot reload stage: no active run");
                    return;
                }

                string stageId = _runStateService.Current.StageId;
                ResetRunStateForStage(_runStateService.Current, stageId);

                _interactionController.Deactivate();
                _interactionLock.Reset();
                _prepareService.Reset();

                _cleanupService.Cleanup();

                _configHotReload.ReloadIfDirty();
                _figureSpawnService.ClearCache();
                _figureStatsFactory.ClearCache();
                _turnPatternFactory.ResetCache();

                _logger.Info($"Reloading stage '{stageId}' in-place");
                await _runHolder.Current.RestartCurrentStageAsync();
            }
            finally
            {
                Interlocked.Exchange(ref _reloading, 0);
            }
        }

        private static void ResetRunStateForStage(PlayerRunStateModel runState, string stageId)
        {
            runState.StageId = stageId;
            foreach (FigureState figure in runState.Figures)
            {
                figure.Location = FigureLocation.InHand();
            }
        }
    }
}
