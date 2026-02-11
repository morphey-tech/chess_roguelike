using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.Scene;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Stage.Messages;
using Project.Gameplay.UI;

namespace Project.Gameplay.Gameplay.Stage.Flow
{
    /// <summary>
    /// Single decision point for stage end transitions (victory/defeat/abort).
    /// </summary>
    public sealed class RunFlowService : IDisposable
    {
        private readonly RunHolder _runHolder;
        private readonly StageReloadService _stageReloadService;
        private readonly IGameUiService _uiService;
        private readonly ISceneService _sceneService;
        private readonly IRunTransitionService _transitionService;
        private readonly ILogger<RunFlowService> _logger;
        private readonly IDisposable _subscription;
        private int _isHandling;

        public RunFlowService(
            RunHolder runHolder,
            StageReloadService stageReloadService,
            IGameUiService uiService,
            ISceneService sceneService,
            IRunTransitionService transitionService,
            ISubscriber<StageCompletedMessage> stageCompletedSubscriber,
            ILogService logService)
        {
            _runHolder = runHolder;
            _stageReloadService = stageReloadService;
            _uiService = uiService;
            _sceneService = sceneService;
            _transitionService = transitionService;
            _logger = logService.CreateLogger<RunFlowService>();
            _subscription = stageCompletedSubscriber.Subscribe(OnStageCompleted);
        }

        private void OnStageCompleted(StageCompletedMessage message)
        {
            HandleStageEnd(message.Result).Forget();
        }

        public async UniTask HandleStageEnd(StageResult result)
        {
            if (Interlocked.Exchange(ref _isHandling, 1) == 1)
                return;

            try
            {
                await _uiService.HideCombatUiAsync();

                switch (result.Outcome)
                {
                    case StageOutcome.Victory:
                        await OnVictory(result);
                        break;
                    case StageOutcome.Defeat:
                        await OnDefeat(result);
                        break;
                    case StageOutcome.Abort:
                        _logger.Info("Stage aborted, no flow transition");
                        break;
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isHandling, 0);
            }
        }

        private async UniTask OnVictory(StageResult result)
        {
            StageFlowAction action = await _uiService.ShowVictoryScreenAsync(result);
            await HandleActionAsync(action, result);
        }

        private async UniTask OnDefeat(StageResult result)
        {
            StageFlowAction action = await _uiService.ShowDefeatScreenAsync(result);
            await HandleActionAsync(action, result);
        }

        private async UniTask HandleActionAsync(StageFlowAction action, StageResult result)
        {
            switch (action)
            {
                case StageFlowAction.NextStage:
                {
                    await _transitionService.PlayTransitionAsync();
                    var run = _runHolder.Current;
                    if (run == null)
                        return;

                    _stageReloadService.PrepareForStageTransition(resetRunStateToHand: true);

                    bool moved = await run.NextStageAsync();
                    if (!moved)
                    {
                        _logger.Info("Run completed after victory, loading hub");
                        await _sceneService.LoadAsync(
                            SceneNames.MainMenu,
                            SceneLoadParams.Default,
                            new SceneTransitionData());
                    }
                    break;
                }
                case StageFlowAction.RestartStage:
                    await _transitionService.PlayTransitionAsync();
                    await _stageReloadService.ReloadCurrentStageAsync();
                    break;
                case StageFlowAction.GoHub:
                    await _transitionService.PlayTransitionAsync();
                    await _sceneService.LoadAsync(
                        SceneNames.MainMenu,
                        SceneLoadParams.Default,
                        new SceneTransitionData());
                    break;
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
