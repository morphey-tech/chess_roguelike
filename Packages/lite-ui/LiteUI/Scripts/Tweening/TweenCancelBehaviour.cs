namespace DG.Tweening
{
    /// <summary>
    /// Stub enum for DOTween Pro UniTask integration.
    /// Defines behavior when a tween await is cancelled.
    /// </summary>
    public enum TweenCancelBehaviour
    {
        Kill,
        KillWithCompleteCallback,
        Complete,
        CompleteWithSequenceCallback,
        CancelAwait
    }

    public static class TweenAsyncExtensions
    {
        public static Cysharp.Threading.Tasks.UniTask AwaitForComplete(
            this Tween tween,
            TweenCancelBehaviour tweenCancelBehaviour = TweenCancelBehaviour.Kill,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
            
            if (tween == null || !tween.IsActive())
            {
                tcs.TrySetResult();
                return tcs.Task;
            }

            tween.OnComplete(() => tcs.TrySetResult());
            tween.OnKill(() => tcs.TrySetResult());

            cancellationToken.Register(() =>
            {
                if (tween.IsActive())
                {
                    switch (tweenCancelBehaviour)
                    {
                        case TweenCancelBehaviour.Kill:
                            tween.Kill();
                            break;
                        case TweenCancelBehaviour.KillWithCompleteCallback:
                            tween.Kill(true);
                            break;
                        case TweenCancelBehaviour.Complete:
                            tween.Complete();
                            break;
                    }
                }
                tcs.TrySetCanceled(cancellationToken);
            });

            return tcs.Task;
        }

        public static Cysharp.Threading.Tasks.UniTask AwaitForRewind(
            this Tween tween,
            TweenCancelBehaviour tweenCancelBehaviour = TweenCancelBehaviour.Kill,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
            
            if (tween == null || !tween.IsActive())
            {
                tcs.TrySetResult();
                return tcs.Task;
            }

            tween.OnRewind(() => tcs.TrySetResult());
            tween.OnKill(() => tcs.TrySetResult());

            cancellationToken.Register(() =>
            {
                if (tween.IsActive())
                {
                    switch (tweenCancelBehaviour)
                    {
                        case TweenCancelBehaviour.Kill:
                            tween.Kill();
                            break;
                        case TweenCancelBehaviour.KillWithCompleteCallback:
                            tween.Kill(true);
                            break;
                    }
                }
                tcs.TrySetCanceled(cancellationToken);
            });

            return tcs.Task;
        }
    }
}

