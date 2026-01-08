using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LiteUI.Common.Extensions
{
    public static class AsyncOperationHandleExtensions
    {
        public static async UniTask<T> ToUniTaskWithResult<T>(this AsyncOperationHandle<T> handle, 
            CancellationToken cancellationToken = default)
        {
            if (handle.IsDone)
            {
                return handle.Result;
            }

            var tcs = new UniTaskCompletionSource<T>();
            
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    tcs.TrySetResult(op.Result);
                }
                else
                {
                    tcs.TrySetException(op.OperationException ?? 
                        new System.Exception($"AsyncOperation failed with status: {op.Status}"));
                }
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            }

            return await tcs.Task;
        }

        public static async UniTask ToUniTaskVoid(this AsyncOperationHandle handle,
            CancellationToken cancellationToken = default)
        {
            if (handle.IsDone)
            {
                return;
            }

            var tcs = new UniTaskCompletionSource();
            
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetException(op.OperationException ?? 
                        new System.Exception($"AsyncOperation failed with status: {op.Status}"));
                }
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            }

            await tcs.Task;
        }
    }
}

