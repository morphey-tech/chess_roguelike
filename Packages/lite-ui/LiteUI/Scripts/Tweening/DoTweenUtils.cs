using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace LiteUI.Tweening
{
    public static class DoTweenUtils
    {
        public static UniTask PlayAllForwardAsync(List<DOTweenAnimation> animations,
                                                  bool forceRecreate = false,
                                                  bool forceRewind = false,
                                                  TweenCancelBehaviour tweenCancelBehaviour = TweenCancelBehaviour.CancelAwait,
                                                  CancellationToken cancellationToken = default)
        {
            List<UniTask> tasks = new();
            foreach (DOTweenAnimation animation in animations) {
                if (forceRecreate || animation.tween == null) {
                    animation.RewindThenRecreateTween();
                } else if (forceRewind) {
                    animation.tween.Rewind();
                }
                animation.tween.PlayForward();
                tasks.Add(animation.tween!.AwaitForCompleteCancelIfKilled(tweenCancelBehaviour, cancellationToken));
            }
            return UniTask.WhenAll(tasks);
        }

        public static void PlayAllForward(List<DOTweenAnimation> animations, bool forceRecreate = false, bool forceRewind = false)
        {
            foreach (DOTweenAnimation animation in animations) {
                if (forceRecreate || animation.tween == null) {
                    animation.RewindThenRecreateTween();
                } else if (forceRewind) {
                    animation.tween.Rewind();
                }
                animation.tween.PlayForward();
            }
        }

        public static UniTask PlayAllBackwardsAsync(List<DOTweenAnimation> animations,
                                                    TweenCancelBehaviour tweenCancelBehaviour = TweenCancelBehaviour.CancelAwait,
                                                    CancellationToken cancellationToken = default)
        {
            List<UniTask> tasks = new();
            foreach (DOTweenAnimation animation in animations) {
                if (animation.tween == null) {
                    animation.RewindThenRecreateTween();
                }
                animation.tween.PlayBackwards();
                tasks.Add(animation.tween!.AwaitForRewindCancelIfKilled(tweenCancelBehaviour, cancellationToken));
            }
            return UniTask.WhenAll(tasks);
        }

        public static void PlayAllBackwards(List<DOTweenAnimation> animations)
        {
            foreach (DOTweenAnimation animation in animations) {
                if (animation.tween == null) {
                    animation.RewindThenRecreateTween();
                }
                animation.tween.PlayBackwards();
            }
        }

        public static void KillAll(List<DOTweenAnimation> animations, bool complete = false)
        {
            foreach (DOTweenAnimation animation in animations) {
                if (animation.tween == null) {
                    continue;
                }
                if (!animation.tween.active) {
                    continue;
                }
                animation.tween.Kill(complete);
            }
        }
    }
}
