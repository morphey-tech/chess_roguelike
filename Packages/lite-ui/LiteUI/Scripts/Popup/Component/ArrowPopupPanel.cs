using System;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LiteUI.Popup.Model;
using LiteUI.UI.Service;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace LiteUI.Popup.Component
{
    public class ArrowPopupPanel : MonoBehaviour
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<ArrowPopupPanel>();

        [SerializeField]
        private VerticalLayoutGroup _layoutGroup = null!;
        [SerializeField]
        private RectTransform _panel = null!;
        [SerializeField]
        private RectTransform _content = null!;
        [SerializeField]
        private RectTransform _leftArrow = null!;
        [SerializeField]
        private RectTransform _rightArrow = null!;
        [SerializeField]
        private RectTransform _topArrow = null!;
        [SerializeField]
        private RectTransform _bottomArrow = null!;
        [SerializeField]
        private Vector2 _maxArrowOffset = new Vector2(15, 10);
        [SerializeField]
        private DOTweenAnimation? _showHideAnimation;

        private ScreenLayout _screenLayout = null!;

        private RectTransform? _target;
        private Vector2 _offset;
        private PopupAlign _defaultAlign;
        private PopupAlign _currentAlign;

        private Vector2 _prevTargetPosition;
        private Rect _prevPanelRect;

        private bool _screenLayoutLogged;

        public void Init(ScreenLayout screenLayout)
        {
            _screenLayout = screenLayout;
        }

        public async UniTask Show(RectTransform target, PopupAlign defaultAlign, Vector2 offset)
        {
            _target = target;
            _defaultAlign = defaultAlign;
            _currentAlign = defaultAlign;
            _offset = offset;

            InvalidatePosition();

            if (_showHideAnimation != null) {
                _panel.localScale = Vector3.zero;
                _showHideAnimation.DOPlay();
                await UniTask.Delay(TimeSpan.FromSeconds(_showHideAnimation.delay + _showHideAnimation.duration));
            }
        }

        public async UniTask Hide()
        {
            if (_showHideAnimation != null) {
                _showHideAnimation.endValueFloat = 0;
                _showHideAnimation.RewindThenRecreateTweenAndPlay();
                await UniTask.Delay(TimeSpan.FromSeconds(_showHideAnimation.delay + _showHideAnimation.duration + 0.1f));
            }
        }

        private void LateUpdate()
        {
            if (_screenLayout == null) {
                if (!_screenLayoutLogged) {
                    _logger.Warn($"No screenLayout set or it`s destroyed");
                    _screenLayoutLogged = true;
                }
                return;
            }
            InvalidatePosition();
        }

        private void InvalidatePosition()
        {
            if (_target == null || (_prevTargetPosition.Equals(_target.position) && _prevPanelRect.Equals(_content.rect))) {
                return;
            }
            _prevTargetPosition = _target.position;
            _prevPanelRect = _content.rect;

            Bounds targetBounds = GetTargetBounds();

            Vector2 popupPosition = targetBounds.center + GetPopupOffsetForTargetWithAlign(_currentAlign, targetBounds);
            if (!IsPopupInScreenByAlign(popupPosition, _currentAlign)) {
                _currentAlign = _currentAlign.InversedAlign();
                popupPosition = targetBounds.center + GetPopupOffsetForTargetWithAlign(_currentAlign, targetBounds);
            }

            UpdateAlign(_currentAlign);
            SelectArrow(_currentAlign);

            Vector2 clampedPosition = ClampByScreen(popupPosition, _currentAlign);
            _panel.position = clampedPosition * _screenLayout.CanvasScale;

            PutArrow(_currentAlign, popupPosition - clampedPosition);
        }

        private void SelectArrow(PopupAlign align)
        {
            _leftArrow.gameObject.SetActive(align == PopupAlign.RIGHT);
            _rightArrow.gameObject.SetActive(align == PopupAlign.LEFT);
            _topArrow.gameObject.SetActive(align == PopupAlign.BOTTOM);
            _bottomArrow.gameObject.SetActive(align == PopupAlign.TOP);
        }

        private void UpdateAlign(PopupAlign align)
        {
            if (align == PopupAlign.RIGHT) {
                _panel.pivot = new Vector2(0f, 0.5f);
                _layoutGroup.childAlignment = TextAnchor.UpperLeft;
            } else if (align == PopupAlign.LEFT) {
                _panel.pivot = new Vector2(1f, 0.5f);
                _layoutGroup.childAlignment = TextAnchor.LowerRight;
            } else if (align == PopupAlign.TOP) {
                _panel.pivot = new Vector2(0.5f, 0f);
                _layoutGroup.childAlignment = TextAnchor.UpperCenter;
            } else {
                _panel.pivot = new Vector2(0.5f, 1f);
                _layoutGroup.childAlignment = TextAnchor.LowerCenter;
            }
        }

        private void PutArrow(PopupAlign align, Vector3 targetOffset)
        {
            Vector2 contentSize = _content.rect.size;
            switch (align) {
                case PopupAlign.TOP: {
                    float minX = -contentSize.x / 2 + _maxArrowOffset.x + _bottomArrow.sizeDelta.x / 2;
                    float maxX = contentSize.x / 2 - _maxArrowOffset.x - _bottomArrow.sizeDelta.x / 2;
                    if (minX >= maxX) {
                        _bottomArrow.anchoredPosition = new Vector2(0, _bottomArrow.anchoredPosition.y);
                    } else {
                        float topAlignX = Mathf.Clamp(targetOffset.x, minX, maxX);
                        _bottomArrow.anchoredPosition = new Vector2(topAlignX, _bottomArrow.anchoredPosition.y);
                    }
                    return;
                }
                case PopupAlign.BOTTOM: {
                    float minX = -contentSize.x / 2 + _maxArrowOffset.x + _topArrow.sizeDelta.x / 2;
                    float maxX = contentSize.x / 2 - _maxArrowOffset.x - _topArrow.sizeDelta.x / 2;
                    if (minX >= maxX) {
                        _topArrow.anchoredPosition = new Vector2(0, _bottomArrow.anchoredPosition.y);
                    } else {
                        float bottomAlignX = Mathf.Clamp(targetOffset.x, minX, maxX);
                        _topArrow.anchoredPosition = new Vector2(bottomAlignX, _topArrow.anchoredPosition.y);
                    }
                    return;
                }
                case PopupAlign.LEFT: {
                    float minY = -contentSize.y / 2 + _maxArrowOffset.y + _rightArrow.sizeDelta.y / 2;
                    float maxY = contentSize.y / 2 - _maxArrowOffset.y - _rightArrow.sizeDelta.y / 2;
                    if (minY >= maxY) {
                        _rightArrow.anchoredPosition = new Vector2(_rightArrow.anchoredPosition.x, 0);
                    } else {
                        float leftAlignY = Mathf.Clamp(targetOffset.y, minY, maxY);
                        _rightArrow.anchoredPosition = new Vector2(_rightArrow.anchoredPosition.x, leftAlignY);
                    }
                    return;
                }
                case PopupAlign.RIGHT: {
                    float minY = -contentSize.y / 2 + _maxArrowOffset.y + _leftArrow.sizeDelta.y / 2;
                    float maxY = contentSize.y / 2 - _maxArrowOffset.y - _leftArrow.sizeDelta.y / 2;
                    if (minY >= maxY) {
                        _leftArrow.anchoredPosition = new Vector2(_leftArrow.anchoredPosition.x, 0);
                    } else {
                        float rightAlignY = Mathf.Clamp(targetOffset.y, minY, maxY);
                        _leftArrow.anchoredPosition = new Vector2(_leftArrow.anchoredPosition.x, rightAlignY);
                    }
                    return;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(align), align, null);
            }
        }

        private Vector2 ClampByScreen(Vector2 popupPosition, PopupAlign currentAlign)
        {
            Vector2 screenSize = _screenLayout.SafeArea.size / _screenLayout.CanvasScale;
            switch (currentAlign) {
                case PopupAlign.TOP:
                case PopupAlign.BOTTOM:
                    float popupWidth = _content.rect.size.x / 2;
                    return new Vector2(Mathf.Min(Mathf.Max(popupPosition.x, popupWidth), screenSize.x - popupWidth), popupPosition.y);
                case PopupAlign.LEFT:
                case PopupAlign.RIGHT:
                    float popupHeight = _panel.rect.size.y / 2;
                    return new Vector2(popupPosition.x, Mathf.Min(Mathf.Max(popupPosition.y, popupHeight), screenSize.y - popupHeight));
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentAlign), currentAlign, null);
            }
        }

        private bool IsPopupInScreenByAlign(Vector3 popupPosition, PopupAlign align)
        {
            Vector2 screenSize = _screenLayout.SafeArea.size / _screenLayout.CanvasScale;
            switch (align) {
                case PopupAlign.TOP:
                    return popupPosition.y + (_content.rect.size.y + _layoutGroup.padding.top) / 1 < screenSize.y;
                case PopupAlign.BOTTOM:
                    return popupPosition.y - (_content.rect.size.y + _layoutGroup.padding.bottom) / 1 > 0;
                case PopupAlign.LEFT:
                    return popupPosition.x - (_content.rect.size.x + _layoutGroup.padding.left) / 1 > 0;
                case PopupAlign.RIGHT:
                    return popupPosition.x + (_content.rect.size.x + _layoutGroup.padding.right) / 1 < screenSize.x;
                default:
                    throw new ArgumentOutOfRangeException(nameof(align), align, null);
            }
        }

        private Vector3 GetPopupOffsetForTargetWithAlign(PopupAlign align, Bounds targetBounds)
        {
            switch (align) {
                case PopupAlign.TOP:
                    return new Vector3(0, targetBounds.extents.y - _offset.y, 0);
                case PopupAlign.BOTTOM:
                    return new Vector3(0, -targetBounds.extents.y + _offset.y, 0);
                case PopupAlign.LEFT:
                    return new Vector3(-targetBounds.extents.x + _offset.x, 0, 0);
                case PopupAlign.RIGHT:
                    return new Vector3(targetBounds.extents.x - _offset.x, 0, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(align), align, null);
            }
        }

        private Bounds GetTargetBounds()
        {
            Vector3 targetTransformPosition;
            try {
                targetTransformPosition = _target!.position;
            } catch {
                _logger.Warn($"Error at _targetTransform.get_position, gameObject={gameObject.name}");
                throw;
            }

            Vector2 anchoredPosition = targetTransformPosition / _screenLayout.CanvasScale;
            Vector2 sizeDelta = _target.sizeDelta;
            Vector2 pivot = _target.pivot;
            anchoredPosition.x += (0.5f - pivot.x) * sizeDelta.x;
            anchoredPosition.y += (0.5f - pivot.y) * sizeDelta.y;
            return new Bounds(anchoredPosition, sizeDelta);
        }


    }
}
