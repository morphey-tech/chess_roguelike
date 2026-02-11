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
                PrepareForStageTransition(resetRunStateToHand: false);

                _logger.Info($"Reloading stage '{stageId}' in-place");
                // Fire-and-forget: stage phases run indefinitely (wait for player input).
                // Awaiting here would keep _reloading=1 forever, blocking subsequent reloads.
                _runHolder.Current.RestartCurrentStageAsync().Forget();
            }
            finally
            {
                Interlocked.Exchange(ref _reloading, 0);
            }
        }

        /// <summary>
        /// Prepares runtime systems and visuals for stage switch (next stage / restart).
        /// Ensures old board/prepare/figures are fully cleaned before next stage starts.
        /// </summary>
        public void PrepareForStageTransition(bool resetRunStateToHand = true)
        {
            if (!_runStateService.HasRun || _runStateService.Current == null)
                return;

            if (resetRunStateToHand)
                ResetRunStateForNextStage(_runStateService.Current);

            _interactionController.Deactivate();
            _interactionLock.Reset();
            _prepareService.Reset();

            _cleanupService.Cleanup();

            _configHotReload.ReloadIfDirty();
            _figureSpawnService.ClearCache();
            _figureStatsFactory.ClearCache();
            _turnPatternFactory.ResetCache();
        }

        private static void ResetRunStateForStage(PlayerRunStateModel runState, string stageId)
        {
            runState.StageId = stageId;
            ResetRunStateForNextStage(runState);
        }

        private static void ResetRunStateForNextStage(PlayerRunStateModel runState)
        {
            foreach (FigureState figure in runState.Figures)
                figure.Location = FigureLocation.InHand();
        }
    }
}
