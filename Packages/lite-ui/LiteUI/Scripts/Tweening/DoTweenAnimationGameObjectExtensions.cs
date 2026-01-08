using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LiteUI.Common.Extensions;
using UnityEngine;

namespace LiteUI.Tweening
{
    public static class DoTweenAnimationGameObjectExtensions
    {
        public static List<DOTweenAnimation> GetDOAnimations(this GameObject gameObject, bool includeInactive = false, bool includeChildren = false)
        {
            List<DOTweenAnimation> animations = new();
            animations.AddRange(gameObject.GetComponents<DOTweenAnimation>());
            if (includeChildren) {
                animations.AddRange(gameObject.GetComponentsInChildren<DOTweenAnimation>(includeInactive).Where(c => c.gameObject != gameObject));
            }
            return animations;
        }

        public static List<DOTweenAnimation> GetDOAnimationsById(this GameObject gameObject,
                                                                 string id,
                                                                 bool includeInactive = false,
                                                                 bool includeChildren = false)
        {
            return GetDOAnimations(gameObject, includeInactive, includeChildren).Where(a => a.id == id).ToList();
        }

        public static List<Tween> GetDOAnimationTweens(this GameObject gameObject,
                                                       bool includeInactive = false,
                                                       bool includeChildren = false,
                                                       bool includeActiveTweens = false)
        {
            return GetDOAnimations(gameObject, includeInactive, includeChildren)
                   .Where(a => a.tween != null && (includeActiveTweens || !a.tween.active))
                   .Select(a => a.tween)
                   .ToList();
        }

        public static List<Tween> GetDOAnimationTweensById(this GameObject gameObject,
                                                           string id,
                                                           bool includeInactive = false,
                                                           bool includeChildren = false,
                                                           bool includeActiveTweens = false)
        {
            return GetDOAnimationsById(gameObject, id, includeInactive, includeChildren)
                   .Where(a => a.tween != null && (includeActiveTweens || !a.tween.active))
                   .Select(a => a.tween)
                   .ToList();
        }

        public static void DOAnimationRewindThenRecreateById(this GameObject gameObject,
                                                             string id,
                                                             bool includeInactive = false,
                                                             bool includeChildren = false)
        {
            List<DOTweenAnimation> animations = GetDOAnimationsById(gameObject, id, includeInactive, includeChildren);
            foreach (DOTweenAnimation animation in animations) {
                animation.RewindThenRecreateTween();
            }
        }

        public static void DOAnimationRecreateById(this GameObject gameObject, string id, bool includeInactive = false, bool includeChildren = false)
        {
            List<DOTweenAnimation> animations = GetDOAnimationsById(gameObject, id, includeInactive, includeChildren);
            foreach (DOTweenAnimation animation in animations) {
                animation.RecreateTween();
            }
        }

        public static UniTask DOAnimationPlayForwardByIdAsync(this GameObject gameObject,
                                                              string id,
                                                              bool includeInactive = false,
                                                              bool includeChildren = false,
                                                              bool forceRecreate = false,
                                                              bool forceRewind = false,
                                                              TweenCancelBehaviour tweenCancelBehaviour = TweenCancelBehaviour.CancelAwait,
                                                              CancellationToken cancellationToken = default)
        {
            List<DOTweenAnimation> animations = GetDOAnimationsById(gameObject, id, includeInactive, includeChildren);
            return DoTweenUtils.PlayAllForwardAsync(animations, forceRecreate, forceRewind, tweenCancelBehaviour, cancellationToken);
        }

        public static void DOAnimationPlayForwardById(this GameObject gameObject,
                                                      string id,
                                                      bool includeInactive = false,
                                                      bool includeChildren = false,
                                                      bool forceRecreate = false,
                                                      bool forceRewind = false)
        {
            List<DOTweenAnimation> animations = GetDOAnimationsById(gameObject, id, includeInactive, includeChildren);
            DoTweenUtils.PlayAllForward(animations, forceRecreate, forceRewind);
        }

        public static UniTask DOAnimationPlayBackwardsByIdAsync(this GameObject gameObject,
                                                                string id,
                                                                bool includeInactive = false,
                                                                bool includeChildren = false,
                                                                TweenCancelBehaviour tweenCancelBehaviour = TweenCancelBehaviour.CancelAwait,
                                                                CancellationToken cancellationToken = default)
        {
            List<DOTweenAnimation> animations = GetDOAnimationsById(gameObject, id, includeInactive, includeChildren);
            return DoTweenUtils.PlayAllBackwardsAsync(animations, tweenCancelBehaviour, cancellationToken);
        }

        public static void DOAnimationPlayBackwardsById(this GameObject gameObject,
                                                        string id,
                                                        bool includeInactive = false,
                                                        bool includeChildren = false)
        {
            List<DOTweenAnimation> animations = GetDOAnimationsById(gameObject, id, includeInactive, includeChildren);
            DoTweenUtils.PlayAllBackwards(animations);
        }

        public static void DOAnimationKillById(this GameObject gameObject,
                                               string id,
                                               bool includeInactive = false,
                                               bool includeChildren = false,
                                               bool complete = false)
        {
            List<DOTweenAnimation> animations = GetDOAnimationsById(gameObject, id, includeInactive, includeChildren);
            DoTweenUtils.KillAll(animations, complete);
        }
        
        public static async UniTask SetObjectActiveWithTween(this GameObject targetObject, bool value, string tweenId, CancellationToken token)
        {
            try {
                if (!value) {
                    await targetObject.DOAnimationPlayBackwardsByIdAsync(tweenId, cancellationToken: token);
                }
                if (targetObject.IsNullOrDestroyed()) {
                    return;
                }
                targetObject.SetActive(value);
                if (value) {
                    await UniTask.WaitUntil(() => targetObject.activeSelf && targetObject.activeInHierarchy, cancellationToken: token);
                    await targetObject.DOAnimationPlayForwardByIdAsync(tweenId, forceRewind: true, cancellationToken: token);
                }
            } catch (OperationCanceledException) {
                //Операция отмеена игнорируем
            } finally {
                targetObject.SetActive(value);
            }
        }
    }
}
