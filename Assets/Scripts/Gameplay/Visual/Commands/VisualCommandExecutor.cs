using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using VContainer;

namespace Project.Gameplay.Gameplay.Visual.Commands
{
    /// <summary>
    /// Executes visual commands sequentially. Без таймаутов — зависание = баг, лог + продолжение.
    /// </summary>
    public sealed class VisualCommandExecutor
    {
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

        public async UniTask ExecuteAsync(VisualCommandQueue queue)
        {
            if (queue == null || queue.Commands.Count == 0)
            {
                queue?.Clear();
                return;
            }

            IVisualQueueAppender appender = _presenters.QueueAppender;
            appender?.SetCurrentQueue(queue);

            try
            {
                _logger.Debug($"=== Visual Pipeline Start ({queue.Commands.Count} commands) ===");

                int i = 0;
                while (i < queue.Commands.Count)
                {
                    IVisualCommand command = queue.Commands[i];
                    _logger.Debug($"▶ [{i}] {command.DebugName} (mode={command.Mode})");

                    if (command.Mode == VisualCommandMode.Parallel)
                    {
                        // Собираем все последовательные Parallel команды
                        var parallelCommands = new List<IVisualCommand> { command };
                        int j = i + 1;
                        while (j < queue.Commands.Count && queue.Commands[j].Mode == VisualCommandMode.Parallel)
                        {
                            _logger.Debug($"▶ [{j}] {queue.Commands[j].DebugName} (mode={queue.Commands[j].Mode}) [parallel group]");
                            parallelCommands.Add(queue.Commands[j]);
                            j++;
                        }

                        // Выполняем все Parallel команды параллельно
                        if (parallelCommands.Count > 1)
                        {
                            _logger.Debug($"Executing {parallelCommands.Count} commands in parallel");
                            UniTask[] tasks = parallelCommands.Select(ExecuteSingleCommandAsync).ToArray();
                            await UniTask.WhenAll(tasks);
                        }
                        else
                        {
                            await ExecuteSingleCommandAsync(command);
                        }

                        i = j;
                    }
                    else
                    {
                        try
                        {
                            if (command.Mode == VisualCommandMode.Blocking)
                            {
                                await command.ExecuteAsync(_presenters);
                            }
                            else
                            {
                                RunBackground(command.ExecuteAsync(_presenters), command.DebugName);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Visual command error: {command.DebugName} - {ex.Message}");
                        }

                        i++;
                    }
                }

                _logger.Debug($"=== Visual Pipeline Complete ===");
            }
            finally
            {
                appender?.SetCurrentQueue(null);
            }

            queue.Clear();
        }

        private async UniTask ExecuteSingleCommandAsync(IVisualCommand command)
        {
            try
            {
                await command.ExecuteAsync(_presenters);
            }
            catch (Exception ex)
            {
                _logger.Error($"Visual command error: {command.DebugName} - {ex.Message}");
            }
        }

        public async UniTask ExecuteAsync(IReadOnlyList<IVisualCommand> commands)
        {
            if (commands == null || commands.Count == 0)
                return;
            var queue = new VisualCommandQueue();
            queue.EnqueueRange(commands);
            await ExecuteAsync(queue);
        }

        private void RunBackground(UniTask task, string name)
        {
            task.Forget(ex =>
            {
                _logger.Error($"Background visual command failed: {name}", ex);
            });
        }
    }
}
