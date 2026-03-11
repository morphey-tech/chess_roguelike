using System;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Unity.Debug
{
    public sealed class ReloadStageConsoleCommands : IStartable, IDisposable
    {
        private readonly StageReloadService _stageReloadService;
        private readonly ILogger _logger;
        private bool _registered;

        [Inject]
        private ReloadStageConsoleCommands(
            StageReloadService stageReloadService,
            ILogService logService)
        {
            _stageReloadService = stageReloadService;
            _logger = logService.CreateLogger<ReloadStageConsoleCommands>();
        }

        void IStartable.Start()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#endif

            DebugLogConsole.AddCommand(
                "reload_stage",
                "Reload current battle and stage",
                ReloadStage);

            _registered = true;
            _logger.Info("Debug command registered: reload_stage");
        }

        private void ReloadStage()
        {
            _stageReloadService.ReloadCurrentStageAsync().Forget();
        }

        void IDisposable.Dispose()
        {
            if (!_registered)
            {
                return;
            }
            DebugLogConsole.RemoveCommand("reload_stage");
            _registered = false;
        }
    }
}
