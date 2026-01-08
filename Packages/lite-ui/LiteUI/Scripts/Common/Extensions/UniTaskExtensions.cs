using System;
using Cysharp.Threading.Tasks;

namespace LiteUI.Common.Extensions
{
    public static class UniTaskExtensions
    {
        public static async UniTask Finally(this UniTask task, Action continuationFunction)
        {
            try {
                await task;
            } finally {
                continuationFunction();
            }
        }

        public static async UniTask Finally(this UniTask task, Func<UniTask> continuationFunction)
        {
            try {
                await task;
            } finally {
                await continuationFunction();
            }
        }
    }
}
