using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Save.Service;
using VContainer;

namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageReloadService
    {
        private readonly PlayerRunStateService _runStateService;
        private readonly RunHolder _runHolder;
        private readonly StageRunStateResetService _runStateResetService;
        private readonly StageRuntimeResetService _runtimeResetService;
        private readonly StageCacheResetService _cacheResetService;
        private readonly ILogger _logger;
        
        private int _reloading;

        [Inject]
        private StageReloadService(
            PlayerRunStateService runStateService,
            RunHolder runHolder,
            StageRunStateResetService runStateResetService,
            StageRuntimeResetService runtimeResetService,
            StageCacheResetService cacheResetService,
            ILogService logService)
        {
            _runStateService = runStateService;
            _runHolder = runHolder;
            _runStateResetService = runStateResetService;
            _runtimeResetService = runtimeResetService;
            _cacheResetService = cacheResetService;
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
                _runStateResetService.ResetForStage(_runStateService.Current, stageId);
                PrepareForStageTransition(resetRunStateToHand: false);

                // Fire-and-forget: stage phases run indefinitely (wait for player input).
                // Awaiting here would keep _reloading=1 forever, blocking subsequent reloads.
                _runHolder.Current.RestartCurrentStageAsync().Forget();
                _logger.Info($"Reloading stage '{stageId}' in-place");
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
            {
                return;
            }
            if (resetRunStateToHand)
            {
                _runStateResetService.ResetFiguresToHand(_runStateService.Current);
            }
            _runtimeResetService.ResetRuntime();
            _cacheResetService.ResetCaches();
        }
    }
}
