using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Visual;
using VContainer;

namespace Project.Gameplay.Gameplay.Visual.Commands
{
    /// <summary>
    /// Executes visual commands sequentially with timeout protection.
    /// TurnSystem awaits only this service for visual synchronization.
    /// 
    /// USAGE:
    /// 1. Domain logic populates VisualCommandQueue via IVisualCommandSink
    /// 2. TurnSystem calls ExecuteAsync(queue)
    /// 3. Executor awaits each command in order (with timeout)
    /// 4. TurnSystem continues after all visuals complete
    /// </summary>
    public sealed class VisualCommandExecutor
    {
        /// <summary>
        /// Таймаут на одну команду. SpawnPrepareZone (кэш + N слотов/фигур с анимацией) может занимать 6–10 сек.
        /// </summary>
        private const int TimeoutMs = 15000;
        
        private readonly IPresenterProvider _presenters;
        private readonly ILogger<VisualCommandExecutor> _logger;

        [Inject]
        private VisualCommandExecutor(
            IPresenterProvider presenters,
            ILogService logService)
        {
            _presenters = presenters;
            _logger = logService.CreateLogger<VisualCommandExecutor>();
        }

        /// <summary>
        /// Execute all commands from the queue sequentially.
        /// </summary>
        public async UniTask ExecuteAsync(VisualCommandQueue queue)
        {
            await ExecuteAsync(queue.Commands);
            queue.Clear();
        }

        /// <summary>
        /// Execute a list of commands sequentially with timeout protection.
        /// </summary>
        public async UniTask ExecuteAsync(IReadOnlyList<IVisualCommand> commands)
        {
            if (commands.Count == 0)
                return;

            _logger.Debug($"=== Visual Pipeline Start ({commands.Count} commands) ===");

            for (int i = 0; i < commands.Count; i++)
            {
                IVisualCommand command = commands[i];
                _logger.Debug($"▶ [{i}] {command.DebugName}");
                
                bool completed = await ExecuteWithTimeout(command);
                
                if (!completed)
                {
                    _logger.Warning($"⚠️ Visual command timeout: {command.DebugName}");
                }
            }

            _logger.Debug($"=== Visual Pipeline Complete ===");
        }

        private async UniTask<bool> ExecuteWithTimeout(IVisualCommand command)
        {
            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromMilliseconds(TimeoutMs));

            try
            {
                await command.ExecuteAsync(_presenters).AttachExternalCancellation(cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Visual command error: {command.DebugName} - {ex.Message}");
                return false;
            }
        }
    }
}
