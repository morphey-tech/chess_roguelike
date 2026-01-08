using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace LiteUI.Common.Utils
{
    [PublicAPI]
    public static class UniTaskUtils
    {
        public static Action<T> Action<T>(Func<T, UniTaskVoid> asyncAction)
        {
            return complete => asyncAction.Invoke(complete).Forget();
        }

        public static Action<T1, T2> Action<T1, T2>(Func<T1, T2, UniTaskVoid> asyncAction)
        {
            return (arg1, arg2) => asyncAction.Invoke(arg1, arg2).Forget();
        }

        public static Action<T1, T2, T3> Action<T1, T2, T3>(Func<T1, T2, T3, UniTaskVoid> asyncAction)
        {
            return (arg1, arg2, arg3) => asyncAction.Invoke(arg1, arg2, arg3).Forget();
        }

        public static async UniTask WaitForSeconds(float seconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), true);
        }
    }
}
