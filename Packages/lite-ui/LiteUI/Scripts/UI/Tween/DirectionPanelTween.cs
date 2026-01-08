using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LiteUI.Common.Model;
using LiteUI.Common.Utils;
using LiteUI.UI.Attributes;
using LiteUI.UI.Service;
using UnityEngine;

namespace LiteUI.UI.Tween
{
    public class DirectionPanelTween
    {
        private const float DEFAULT_TWEEN_DURATION = 0.4f;

        private readonly RectTransform _panel;
        private readonly float _layoutOffset;
        private readonly float _duration;
        private readonly float _panelOffset;
        private readonly Direction _side;

        private DirectionPanelTween(RectTransform panel, float layoutOffset, Direction side, float duration, float panelOffset)
        {
            _panel = panel;
            _layoutOffset = layoutOffset;
            _side = side;
            _duration = duration;
            _panelOffset = panelOffset;
        }

        public static UniTask DoShow(ScreenLayout screenLayout, DirectionPanelViewModel viewModel)
        {
            DirectionPanelTween tween = new(viewModel.RectTransform, screenLayout.GetOffset(viewModel.Direction), viewModel.Direction,
                                            viewModel.Duration, viewModel.ShowOffset);
            return tween.Show();
        }

        public static UniTask DoShow(ScreenLayout screenLayout,
                                     GameObject panel,
                                     Direction side,
                                     float duration = DEFAULT_TWEEN_DURATION,
                                     float showOffset = 0)
        {
            DirectionPanelTween tween = new(panel.GetComponent<RectTransform>(), screenLayout.GetOffset(side),
                                            side, duration, showOffset);
            return tween.Show();
        }

        public static UniTask DoHide(ScreenLayout screenLayout, DirectionPanelViewModel viewModel)
        {
            DirectionPanelTween tween = new(viewModel.RectTransform, screenLayout.GetOffset(viewModel.Direction),
                                            viewModel.Direction, viewModel.Duration, viewModel.ShowOffset);
            return tween.Hide();
        }

        public static UniTask DoHide(ScreenLayout screenLayout,
                                     GameObject panel,
                                     Direction side,
                                     float duration = DEFAULT_TWEEN_DURATION,
                                     float showOffset = 0)
        {
            DirectionPanelTween tween = new(panel.GetComponent<RectTransform>(), screenLayout.GetOffset(side),
                                            side, duration, showOffset);
            return tween.Hide();
        }

        private async UniTask Show()
        {
            DOTween.Kill(_panel);
            try {
                if (_panel == null) {
                    return;
                }
                _panel.gameObject.SetActive(true);
                float offset = _panelOffset != 0 ? _panelOffset : _layoutOffset;
                await DoShowTween(_panel, _side, _duration, offset).SuppressCancellationThrow();
            } catch (OperationCanceledException) {
                // ничего не делаем
            }
        }

        private async UniTask Hide()
        {
            DOTween.Kill(_panel);
            try {
                await DoHideTween(_panel, _side, _duration);
                if (_panel == null) {
                    return;
                }
                _panel.gameObject.SetActive(false);
            } catch (OperationCanceledException) {
                // ничего не делаем
            }
        }

        private UniTask DoShowTween(RectTransform panel, Direction side, float duration, float showOffset)
        {
            UniTaskCompletionSource completionSource = new();
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            switch (side) {
                case Direction.LEFT when !IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(showOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.LEFT when IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(showOffset + _layoutOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.RIGHT when !IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(showOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.RIGHT when IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(showOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.UP when IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(showOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.UP when !IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(showOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.DOWN when IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(showOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.DOWN when !IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(showOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
            return completionSource.Task;
        }

        private UniTask DoHideTween(RectTransform panel, Direction side, float duration)
        {
            UniTaskCompletionSource completionSource = new();
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            switch (side) {
                case Direction.LEFT when IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(-(rectTransform.sizeDelta.x + _layoutOffset), duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.LEFT when !IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(-(rectTransform.sizeDelta.x + _layoutOffset), duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.RIGHT when IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(rectTransform.sizeDelta.x + _layoutOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.RIGHT when !IsXAnchorRight(rectTransform):
                    rectTransform.DOAnchorPosX(rectTransform.sizeDelta.x + _layoutOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.UP when !IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(rectTransform.sizeDelta.y + _layoutOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.UP when IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(rectTransform.sizeDelta.y + _layoutOffset, duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.DOWN when !IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(-(rectTransform.sizeDelta.y + _layoutOffset), duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                case Direction.DOWN when IsYAnchorUp(rectTransform):
                    rectTransform.DOAnchorPosY(-(rectTransform.sizeDelta.y + _layoutOffset), duration)
                                 .OnKill(() => completionSource.TrySetCanceled())
                                 .OnComplete(() => completionSource.TrySetResult());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
            return completionSource.Task;
        }

        private bool IsXAnchorRight(RectTransform rectTransform)
        {
            return MathUtils.IsFloatEquals(rectTransform.pivot.x, 1f);
        }

        private bool IsYAnchorUp(RectTransform rectTransform)
        {
            return MathUtils.IsFloatEquals(rectTransform.pivot.y, 1f);
        }
    }
}
