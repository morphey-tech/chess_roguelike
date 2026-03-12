using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.Scene;
using Project.Gameplay.Gameplay.Bootstrap;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Stage.Messages;
using Project.Gameplay.UI;
using VContainer;
using IInitializable = VContainer.Unity.IInitializable;

namespace Project.Gameplay.Gameplay.Stage.Flow
{
    /// <summary>
    /// Single decision point for stage end transitions (victory/defeat/abort).
    /// </summary>
    public sealed class RunFlowService : IInitializable, IDisposable
    {
        private readonly RunHolder _runHolder;
        private readonly StageReloadService _stageReloadService;
        private readonly IGameUiService _uiService;
        private readonly ISceneService _sceneService;
        private readonly IRunTransitionService _transitionService;
        private readonly ISubscriber<string, StagePhaseMessage> _stagePhaseSubscriber;
        private readonly ISubscriber<ForceStageEndMessage> _forceStageEndSubscriber;
        private readonly ILogger<RunFlowService> _logger;

        private IDisposable _disposable;
        private int _isHandling;

        [Inject]
        private RunFlowService(
            RunHolder runHolder,
            StageReloadService stageReloadService,
            IGameUiService uiService,
            ISceneService sceneService,
            IRunTransitionService transitionService,
            ISubscriber<string, StagePhaseMessage> stagePhaseSubscriber,
            ISubscriber<ForceStageEndMessage> forceStageEndSubscriber,
            ILogService logService, IDisposable disposable)
        {
            _runHolder = runHolder;
            _stageReloadService = stageReloadService;
            _uiService = uiService;
            _sceneService = sceneService;
            _transitionService = transitionService;
            _stagePhaseSubscriber = stagePhaseSubscriber;
            _forceStageEndSubscriber = forceStageEndSubscriber;
            _disposable = disposable;
            _logger = logService.CreateLogger<RunFlowService>();
        }

        void IInitializable.Initialize()
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            _stagePhaseSubscriber.Subscribe(StagePhaseMessage.STAGE_COMPLETED, OnStagePhase).AddTo(bag);
            _forceStageEndSubscriber.Subscribe(OnForceStageEnd).AddTo(bag);
            _disposable = bag.Build();
        }

        private void OnStagePhase(StagePhaseMessage message)
        {
            HandleStageEnd(message.Result).Forget();
        }
        
        private void OnForceStageEnd(ForceStageEndMessage message)
        {
            HandleStageEnd(new StageResult(message.Outcome, turnCount: 0, enemiesKilled: 0)).Forget();
        }

        public async UniTask HandleStageEnd(StageResult result)
        {
            if (Interlocked.Exchange(ref _isHandling, 1) == 1)
            {
                return;
            }

            try
            {
                await _uiService.HideBattlePhase();

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
                    Run.Run run = _runHolder.Current;

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

        void IDisposable.Dispose()
        {
            _disposable?.Dispose();
        }

    }
}
