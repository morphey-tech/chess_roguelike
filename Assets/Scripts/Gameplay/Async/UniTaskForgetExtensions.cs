using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;

namespace Project.Gameplay.Gameplay.Async
{
    public static class UniTaskForgetExtensions
    {
        public static void ForgetLogged(
            this UniTask task,
            ILogger logger,
            string errorMessage,
            CancellationToken cancellationToken = default)
        {
            task.Forget(ex =>
            {
                if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
                    return;
                logger?.Error(errorMessage, ex);
            });
        }
    }
}
