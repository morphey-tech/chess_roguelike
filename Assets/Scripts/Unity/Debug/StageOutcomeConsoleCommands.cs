using System;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using VContainer.Unity;

namespace Project.Unity.Unity.Debug
{
    public sealed class StageOutcomeConsoleCommands : IStartable, IDisposable
    {
        private readonly RunFlowService _runFlowService;
        private readonly ILogger _logger;
        private bool _registered;

        public StageOutcomeConsoleCommands(
            RunFlowService runFlowService,
            ILogService logService)
        {
            _runFlowService = runFlowService;
            _logger = logService.CreateLogger<StageOutcomeConsoleCommands>();
        }

        public void Start()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#endif
            DebugLogConsole.AddCommand("stage_win", "Trigger stage victory flow", TriggerWin);
            DebugLogConsole.AddCommand("stage_lose", "Trigger stage defeat flow", TriggerDefeat);
            _registered = true;
            _logger.Info("Debug commands registered: stage_win, stage_lose");
        }

        private void TriggerWin()
        {
            _runFlowService
                .HandleStageEnd(new StageResult(StageOutcome.Victory, turnCount: 1, enemiesKilled: 0))
                .Forget();
        }

        private void TriggerDefeat()
        {
            _runFlowService
                .HandleStageEnd(new StageResult(StageOutcome.Defeat, turnCount: 1, enemiesKilled: 0))
                .Forget();
        }

        public void Dispose()
        {
            if (!_registered)
                return;

            DebugLogConsole.RemoveCommand("stage_win");
            DebugLogConsole.RemoveCommand("stage_lose");
            _registered = false;
        }
    }
}
