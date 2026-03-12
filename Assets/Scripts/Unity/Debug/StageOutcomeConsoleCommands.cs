using System;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Messages;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Unity.Debug
{
    public sealed class StageOutcomeConsoleCommands : IStartable, IDisposable
    {
        private readonly IPublisher<ForceStageEndMessage> _publisher;
        private readonly ILogger _logger;
        private bool _registered;

        [Inject]
        private StageOutcomeConsoleCommands(
            IPublisher<ForceStageEndMessage> publisher,
            ILogService logService)
        {
            _publisher = publisher;
            _logger = logService.CreateLogger<StageOutcomeConsoleCommands>();
        }

        void IStartable.Start()
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
            _publisher.Publish(new ForceStageEndMessage(StageOutcome.Victory));
        }

        private void TriggerDefeat()
        {
            _publisher.Publish(new ForceStageEndMessage(StageOutcome.Defeat));
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
