using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace LiteUI.Tweening
{
    public static class DoTweenAsyncExtensions
    {
        public static async UniTask AwaitForCompleteCancelIfKilled(this Tween tween,
                                                                   TweenCancelBehaviour tweenCancelBehaviour,
                                                                   CancellationToken cancellationToken = default)
        {
            CancellationTokenSource? linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tween.onKill += OnKill;
            tween.onUpdate += OnUpdate;
            try {
                await tween.AwaitForComplete(tweenCancelBehaviour, linkedTokenSource.Token);
            } finally {
                tween.onKill -= OnKill;
                tween.onUpdate -= OnUpdate;
                linkedTokenSource.Dispose();
            }

            void OnKill()
            {
                if (tween.IsComplete()) {
                    return;
                }
                linkedTokenSource.Cancel();
            }

            void OnUpdate()
            {
                if (!tween.isBackwards) {
                    return;
                }
                linkedTokenSource.Cancel();
            }
        }

        public static async UniTask AwaitForRewindCancelIfKilled(this Tween tween,
                                                                 TweenCancelBehaviour tweenCancelBehaviour,
                                                                 CancellationToken cancellationToken = default)
        {
            if (tween.position == 0) {
                return;
            }
            
            CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tween.onKill += OnKill;
            tween.onUpdate += OnUpdate;
            try {
                await tween.AwaitForRewind(tweenCancelBehaviour, linkedTokenSource.Token);
            } finally {
                tween.onKill -= OnKill;
                tween.onUpdate -= OnUpdate;
                linkedTokenSource.Dispose();
            }

            if (tween.position != 0) {
                throw new OperationCanceledException();
            }

            void OnKill()
            {
                if (tween.IsComplete()) {
                    return;
                }
                linkedTokenSource.Cancel();
            }

            void OnUpdate()
            {
                if (tween.isBackwards) {
                    return;
                }
                linkedTokenSource.Cancel();
            }
        }
    }
}
